using System.Reactive;
using System.Text;
using Core.Clients;
using Core.Models;
using ReactiveUI;
using MessageBox_Core;
using Core.Models.NoProtocol;
using System.Text.RegularExpressions;

namespace ViewModels.NoProtocol
{
    public class NoProtocol_VM : ReactiveObject
    {
        public static NoProtocol_VM? Instance { get; private set; }

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

        private readonly IMessageBox _messageBox;

        private readonly NoProtocol_Mode_Normal_VM Mode_Normal_VM;
        private readonly NoProtocol_Mode_Cycle_VM Mode_Cycle_VM;


        public NoProtocol_VM(IMessageBox messageBox)
        {
            _messageBox = messageBox;

            Model = ConnectedHost.Model;

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;

            Model.NoProtocol.Model_DataReceived += NoProtocol_Model_DataReceived;
            Model.NoProtocol.Model_ErrorInReadThread += NoProtocol_Model_ErrorInReadThread;

            Command_ClearRX = ReactiveCommand.Create(() => { RX?.Clear(); RX_String = string.Empty; });

            Mode_Normal_VM = new NoProtocol_Mode_Normal_VM(messageBox, NoProtocol_Send, GetMessageString, GetValidatedByteString);
            Mode_Cycle_VM = new NoProtocol_Mode_Cycle_VM(messageBox, ConvertToBytes, GetMessageString, GetValidatedByteString);

            Instance = this;

            this.WhenAnyValue(x => x.IsCycleMode)
                .Subscribe(_ =>
                {
                    if (!IsCycleMode)
                    {
                        Mode_Cycle_VM.StopPolling();
                    }

                    CurrentModeViewModel = IsCycleMode ? Mode_Cycle_VM : Mode_Normal_VM;
                });
        }
        public async Task NoProtocol_Send(bool isBytes, string Data, bool enableCR, bool enableLF)
        {
            if (isBytes)
            {
                await Model.NoProtocol.SendBytes(ConvertToBytes(Data));
                return;
            }

            await Model.NoProtocol.SendString(Data, enableCR, enableLF);
        }

        private string GetMessageString(string message, bool isBytesString)
        {
            if (isBytesString)
            {
                return string.Join(" ", Model.NoProtocol.HostEncoding.GetBytes(message).Select(x => x.ToString("X2"))); ;
            }

            return Model.NoProtocol.HostEncoding.GetString(ConvertToBytes(message));
        }

        private byte[] ConvertToBytes(string message)
        {
            message = message.Replace(" ", string.Empty);

            byte[] bytesToSend = new byte[message.Length / 2 + message.Length % 2];

            string byteString;

            for (int i = 0; i < bytesToSend.Length; i++)
            {
                if (i * 2 + 2 > message.Length)
                    byteString = "0" + message.Last();
                else
                    byteString = message.Substring(i * 2, 2);

                bytesToSend[i] = Convert.ToByte(byteString, 16);
            }
            return bytesToSend;
        }

        private string GetValidatedByteString(string bytesString)
        {
            return Regex.Replace(bytesString, @"[^0-9a-fA-F\s]", string.Empty).ToUpper();
        }

        private void Model_DeviceIsConnect(object? sender, ConnectArgs e)
        {
            if (e.ConnectedDevice is IPClient)
            {
                InterfaceType = InterfaceType_Ethernet;
            }

            else if (e.ConnectedDevice is SerialPortClient)
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

        private void Model_DeviceIsDisconnected(object? sender, ConnectArgs e)
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
