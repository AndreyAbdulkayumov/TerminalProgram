using ReactiveUI;
using System.Reactive;
using System.Text;
using MessageBox_Core;
using Core.Clients;
using Core.Models;
using Core.Models.NoProtocol.DataTypes;
using Core.Models.Settings.FileTypes;
using Core.Clients.DataTypes;
using ViewModels.NoProtocol.DataTypes;
using ViewModels.Helpers;
using Services.Interfaces;

namespace ViewModels.NoProtocol
{
    public class NoProtocol_VM : ReactiveObject
    {
        private object? _currentModeViewModel;

        public object? CurrentModeViewModel
        {
            get => _currentModeViewModel;
            set => this.RaiseAndSetIfChanged(ref _currentModeViewModel, value);
        }

        #region Properties

        private bool ui_IsEnable = false;

        public bool UI_IsEnable
        {
            get => ui_IsEnable;
            set => this.RaiseAndSetIfChanged(ref ui_IsEnable, value);
        }

        private bool _isCycleMode = false;

        public bool IsCycleMode
        {
            get => _isCycleMode;
            set => this.RaiseAndSetIfChanged(ref _isCycleMode, value);
        }

        private const string InterfaceType_Default = "не определен";
        private const string InterfaceType_SerialPort = "Serial Port";
        private const string InterfaceType_Ethernet = "Ethernet";

        private string _interfaceType = InterfaceType_Default;

        public string InterfaceType
        {
            get => _interfaceType;
            set => this.RaiseAndSetIfChanged(ref _interfaceType, value);
        }

        private string? _selectedEncoding;

        public string? SelectedEncoding
        {
            get => _selectedEncoding;
            set => this.RaiseAndSetIfChanged(ref _selectedEncoding, value);
        }

        private string _rx_String = string.Empty;

        public string RX_String
        {
            get => _rx_String;
            set => this.RaiseAndSetIfChanged(ref _rx_String, value);
        }

        private bool _rx_NextLine;

        public bool RX_NextLine
        {
            get => _rx_NextLine;
            set => this.RaiseAndSetIfChanged(ref _rx_NextLine, value);
        }

        private bool _rx_IsByteView = false;

        public bool RX_IsByteView
        {
            get => _rx_IsByteView;
            set => this.RaiseAndSetIfChanged(ref _rx_IsByteView, value);
        }

        #endregion

        public ReactiveCommand<Unit, Unit> Command_ClearRX { get; }

        private const int MaxCapacity = 3000;

        // Делаем эти значения емкости одинаковыми, чтобы не тратить ресурсы на дополнительное выделение памяти.
        private readonly StringBuilder RX = new StringBuilder(MaxCapacity, MaxCapacity);

        private const string BytesSeparator = " ";
        private const string ElementSeparatorInCycleMode = "  ";

        private readonly ConnectedHost Model;

        private readonly IMessageBoxMainWindow _messageBox;

        private readonly NoProtocol_Mode_Normal_VM _normalMode_VM;
        private readonly NoProtocol_Mode_Cycle_VM _cycleMode_VM;

        public NoProtocol_VM(IMessageBoxMainWindow messageBox,
            NoProtocol_Mode_Normal_VM normalMode_VM, NoProtocol_Mode_Cycle_VM cycleMode_VM)
        {
            _messageBox = messageBox ?? throw new ArgumentNullException(nameof(messageBox));

            _normalMode_VM = normalMode_VM ?? throw new ArgumentNullException(nameof(normalMode_VM));
            _cycleMode_VM = cycleMode_VM ?? throw new ArgumentNullException(nameof(cycleMode_VM));

            Model = ConnectedHost.Model;

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;

            Model.NoProtocol.Model_DataReceived += NoProtocol_Model_DataReceived;
            Model.NoProtocol.Model_ErrorInReadThread += NoProtocol_Model_ErrorInReadThread;

            MessageBus.Current.Listen<NoProtocolSendMessage>()
                .Subscribe(async message =>
                {
                    await Receive_SendMessage_Handler(message);
                });

            MessageBus.Current.Listen<List<MacrosCommandNoProtocol>>()
                .Subscribe(async message =>
                {
                    await Receive_ListMessage_Handler(message);
                });

            Command_ClearRX = ReactiveCommand.Create(() => { RX?.Clear(); RX_String = string.Empty; });

            this.WhenAnyValue(x => x.IsCycleMode)
                .Subscribe(_ =>
                {
                    if (!IsCycleMode)
                    {
                        _cycleMode_VM.StopPolling();
                    }

                    CurrentModeViewModel = IsCycleMode ? _cycleMode_VM : _normalMode_VM;
                });
        }

        private async Task Receive_SendMessage_Handler(NoProtocolSendMessage message)
        {
            try
            {
                byte[] buffer = CreateSendBuffer(message.IsBytes, message.Message, message.EnableCR, message.EnableLF, message.SelectedEncoding);

                await Model.NoProtocol.SendBytes(buffer);
            }
            
            catch (Exception error)
            {
                _messageBox.Show(error.Message, MessageType.Error);
            }
        }

        private async Task Receive_ListMessage_Handler(List<MacrosCommandNoProtocol> message)
        {
            try
            {
                foreach (var command in message)
                {
                    if (command.Content == null)
                        continue;

                    byte[] buffer = CreateSendBuffer(
                        command.Content.IsByteString, 
                        command.Content.Message, 
                        command.Content.EnableCR, 
                        command.Content.EnableLF,
                        AppEncoding.GetEncoding(command.Content.MacrosEncoding)
                        );

                    await Model.NoProtocol.SendBytes(buffer);
                }
            }

            catch (Exception error)
            {
                _messageBox.Show(error.Message, MessageType.Error);
            }
        }

        public static byte[] CreateSendBuffer(bool isBytes, string? message, bool enableCR, bool enableLF, Encoding encoding)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new Exception("Не заданы данные для отправки.");
            }

            List<byte> buffer = new List<byte>(
                isBytes ?
                    StringByteConverter.ByteStringToByteArray(message) :
                    encoding.GetBytes(message)
                    );

            if (enableCR == true)
            {
                buffer.Add((byte)'\r');
            }

            if (enableLF == true)
            {
                buffer.Add((byte)'\n');
            }

            return buffer.ToArray();
        }

        private void Model_DeviceIsConnect(object? sender, IConnection? e)
        {
            if (e is IPClient)
            {
                InterfaceType = InterfaceType_Ethernet;
            }

            else if (e is SerialPortClient)
            {
                InterfaceType = InterfaceType_SerialPort;
            }

            else
            {
                _messageBox.Show("Задан неизвестный тип подключения.", MessageType.Error);
                return;
            }

            UI_IsEnable = true;
        }        

        private void Model_DeviceIsDisconnected(object? sender, IConnection? e)
        {
            InterfaceType = InterfaceType_Default;

            UI_IsEnable = false;
        }

        private void NoProtocol_Model_DataReceived(object? sender, NoProtocolDataReceivedEventArgs e)
        {
            string stringData;

            if (RX_IsByteView)
            {
                stringData = BitConverter.ToString(e.RawData).Replace("-", BytesSeparator) + BytesSeparator;
            }

            else
            {
                stringData = ConnectedHost.GlobalEncoding.GetString(e.RawData);
            }

            if (e.DataWithDebugInfo != null)
            {
                e.DataWithDebugInfo[e.DataIndex] = stringData;
                stringData = string.Join(ElementSeparatorInCycleMode, e.DataWithDebugInfo);
            }

            if (RX_NextLine)
            {
                stringData += Environment.NewLine;
            }

            if (RX.Length + stringData.Length > RX.MaxCapacity)
            {
                RX.Remove(0, RX.Length + stringData.Length - RX.MaxCapacity);
            }

            RX.Append(stringData);

            RX_String = RX.ToString();
        }

        private void NoProtocol_Model_ErrorInReadThread(object? sender, string e)
        {
            _messageBox.Show(e, MessageType.Error);
        }
    }
}
