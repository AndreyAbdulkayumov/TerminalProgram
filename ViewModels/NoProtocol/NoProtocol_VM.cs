using System.Reactive;
using System.Text;
using Core.Clients;
using Core.Models;
using ReactiveUI;
using MessageBox_Core;
using Core.Models.Settings;
using System.Globalization;

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
        
        private StringBuilder RX;

        private bool _rx_NextLine;

        public bool RX_NextLine
        {
            get => _rx_NextLine;
            set => this.RaiseAndSetIfChanged(ref _rx_NextLine, value);
        }

        #endregion


        public ReactiveCommand<Unit, Unit> Command_ClearRX { get; }


        private readonly ConnectedHost Model;
        private readonly Model_Settings SettingsFile;

        private readonly Action<string, MessageType> Message;

        private readonly NoProtocol_Mode_Normal_VM Mode_Normal_VM;
        private readonly NoProtocol_Mode_Cycle_VM Mode_Cycle_VM;


        public NoProtocol_VM(Action<string, MessageType> messageBox)
        {
            Message = messageBox;

            Model = ConnectedHost.Model;
            SettingsFile = Model_Settings.Model;

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

        private void SetRXFieldCapacity()
        {
            //int capacity;

            //if (int.TryParse(SettingsFile.Settings?.ReceiveBufferSize, NumberStyles.Integer, CultureInfo.InvariantCulture, out capacity) == false)
            //{
            //    capacity = Convert.ToInt32(DeviceData.ReceiveBufferSize_Default);
            //}

            //string oldData = RX != null ? RX.ToString() : string.Empty;

            //if (oldData.Length > capacity)
            //{
            //    oldData = oldData.Substring(0, capacity);
            //}

            // Делаем эти значения емкости одинаковыми, чтобы не тратить ресурсы на дополнительное выделение памяти.
            RX = new StringBuilder(3000, 3000);

            //RX.Append(oldData);
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

            SetRXFieldCapacity();

            UI_IsEnable = true;
        }        

        private void Model_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            InterfaceType = InterfaceType_Default;

            UI_IsEnable = false;
        }

        private void NoProtocol_Model_DataReceived(object? sender, string e)
        {
            if (RX_NextLine)
            {
                e += "\n";
            }

            if (RX.Length + e.Length > RX.MaxCapacity)
            {
                RX.Remove(0, RX.Length + e.Length - RX.MaxCapacity);
            }

            RX.Append(e);

            RX_String = RX.ToString();
        }

        private void NoProtocol_Model_ErrorInReadThread(object? sender, string e)
        {
            Message.Invoke(e, MessageType.Error);
        }
    }
}
