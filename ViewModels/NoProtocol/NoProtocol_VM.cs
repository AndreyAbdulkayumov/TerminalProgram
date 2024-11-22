using System.Reactive;
using System.Text;
using Core.Clients;
using Core.Models;
using ReactiveUI;
using MessageBox_Core;
using Core.Models.NoProtocol;

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

        private readonly Action<string, MessageType> Message;

        private readonly NoProtocol_Mode_Normal_VM Mode_Normal_VM;
        private readonly NoProtocol_Mode_Cycle_VM Mode_Cycle_VM;


        public NoProtocol_VM(Action<string, MessageType> messageBox)
        {
            Message = messageBox;

            Model = ConnectedHost.Model;

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;

            Model.NoProtocol.Model_DataReceived += NoProtocol_Model_DataReceived;
            Model.NoProtocol.Model_ErrorInReadThread += NoProtocol_Model_ErrorInReadThread;

            Command_ClearRX = ReactiveCommand.Create(() => { RX?.Clear(); RX_String = string.Empty; });

            Mode_Normal_VM = new NoProtocol_Mode_Normal_VM(messageBox);
            Mode_Cycle_VM = new NoProtocol_Mode_Cycle_VM(messageBox);

            this.WhenAnyValue(x => x.IsCycleMode)
                .Subscribe(_ =>
                {
                    if (!IsCycleMode)
                    {
                        Mode_Cycle_VM.Start_Stop_Handler(false);
                    }

                    CurrentModeViewModel = IsCycleMode ? Mode_Cycle_VM : Mode_Normal_VM;
                });
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
                Message.Invoke("Задан неизвестный тип подключения.", MessageType.Error);
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
                stringData += RX_IsByteView ? "0A " : "\n";
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
            Message.Invoke(e, MessageType.Error);
        }
    }
}
