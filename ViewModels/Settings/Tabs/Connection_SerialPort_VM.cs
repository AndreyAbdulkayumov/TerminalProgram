using Core.Models.Settings;
using ReactiveUI;
using MessageBox_Core;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Reactive;
using System.Reactive.Linq;
using ViewModels.Validation;
using System.Globalization;

namespace ViewModels.Settings.Tabs
{
    public class Connection_SerialPort_VM : ValidatedDateInput, IValidationFieldInfo
    {
        /************************************/
        //
        //  COM - Ports
        //
        /************************************/

        private string _message_PortNotFound = "Content";

        public string Message_PortNotFound
        {
            get => _message_PortNotFound;
            set => this.RaiseAndSetIfChanged(ref _message_PortNotFound, value);
        }

        private bool _message_PortNotFound_IsVisible = false;

        public bool Message_PortNotFound_IsVisible
        {
            get => _message_PortNotFound_IsVisible;
            set => this.RaiseAndSetIfChanged(ref _message_PortNotFound_IsVisible, value);
        }

        private ObservableCollection<string> _serialPorts = new ObservableCollection<string>();

        public ObservableCollection<string> SerialPorts
        {
            get => _serialPorts;
            set => this.RaiseAndSetIfChanged(ref _serialPorts, value);
        }

        private string? _selected_SerialPort;

        public string? Selected_SerialPort
        {
            get => _selected_SerialPort;
            set => this.RaiseAndSetIfChanged(ref _selected_SerialPort, value);
        }

        /************************************/
        //
        //  BaudRate
        //
        /************************************/

        private readonly ObservableCollection<string> _baudRate = new ObservableCollection<string>()
        {
            "4800", "9600", "19200", "38400", "57600", "115200"
        };

        public ObservableCollection<string> BaudRate
        {
            get => _baudRate;
        }

        private string? _selected_BaudRate;

        public string? Selected_BaudRate
        {
            get => _selected_BaudRate;
            set => this.RaiseAndSetIfChanged(ref _selected_BaudRate, value);
        }

        private bool _baudRate_IsCustom = false;

        public bool BaudRate_IsCustom
        {
            get => _baudRate_IsCustom;
            set => this.RaiseAndSetIfChanged(ref _baudRate_IsCustom, value);
        }

        private string? _custom_BaudRate_Value = string.Empty;

        public string? Custom_BaudRate_Value
        {
            get => _custom_BaudRate_Value;
            set
            {
                this.RaiseAndSetIfChanged(ref _custom_BaudRate_Value, value);
                ValidateInput(nameof(Custom_BaudRate_Value), value);
            }
        }

        /************************************/
        //
        //  Parity
        //
        /************************************/

        private readonly ObservableCollection<string> _parity = new ObservableCollection<string>()
        {
            "None", "Even", "Odd", "Space", "Mark"
        };

        public ObservableCollection<string> Parity
        {
            get => _parity;
        }

        private string? _selected_Parity;

        public string? Selected_Parity
        {
            get => _selected_Parity;
            set => this.RaiseAndSetIfChanged(ref _selected_Parity, value);
        }

        /************************************/
        //
        //  DataBits
        //
        /************************************/

        private readonly ObservableCollection<string> _dataBits = new ObservableCollection<string>()
        {
            "5", "6", "7", "8"
        };

        public ObservableCollection<string> DataBits
        {
            get => _dataBits;
        }

        private string? _selected_DataBits;

        public string? Selected_DataBits
        {
            get => _selected_DataBits;
            set => this.RaiseAndSetIfChanged(ref _selected_DataBits, value);
        }

        /************************************/
        //
        //  StopBits
        //
        /************************************/

        private readonly ObservableCollection<string> _stopBits = new ObservableCollection<string>()
        {
            "1", "1.5", "2"
        };

        public ObservableCollection<string> StopBits
        {
            get => _stopBits;
        }

        private string? _selected_StopBits;

        public string? Selected_StopBits
        {
            get => _selected_StopBits;
            set => this.RaiseAndSetIfChanged(ref _selected_StopBits, value);
        }

        public ReactiveCommand<Unit, Unit> Command_ReScan_SerialPorts { get; }

        private readonly Action<string, MessageType> Message;

        private readonly Model_Settings SettingsFile;


        public Connection_SerialPort_VM(Settings_VM main_VM)
        {
            main_VM._settingsFileChanged += Main_VM_SettingsFileChanged;

            Message = main_VM.Message;

            SettingsFile = Model_Settings.Model;

            Command_ReScan_SerialPorts = ReactiveCommand.Create(() =>
            {
                if (SettingsFile.Settings == null)
                {
                    throw new Exception("Не инициализирован файл настроек.");
                }

                ReScan_SerialPorts(SettingsFile.Settings.Connection_SerialPort);
            });

            Command_ReScan_SerialPorts.ThrownExceptions.Subscribe(error => Message.Invoke(error.Message, MessageType.Error));
        }

        public string GetFieldViewName(string fieldName)
        {
            switch (fieldName)
            {
                case nameof(Custom_BaudRate_Value):
                    return "Custom BaudRate";

                default:
                    return fieldName;
            }
        }

        private void Main_VM_SettingsFileChanged(object? sender, EventArgs e)
        {
            try
            {
                if (SettingsFile.Settings == null)
                {
                    throw new Exception("Не инициализирован файл настроек.");
                }

                ReScan_SerialPorts(SettingsFile.Settings.Connection_SerialPort);

                if (SettingsFile.Settings.Connection_SerialPort == null)
                {
                    Selected_SerialPort = null;
                    Message_PortNotFound = "Порт не задан";
                    Message_PortNotFound_IsVisible = true;

                    Selected_SerialPort = null;

                    Selected_BaudRate = null;
                    BaudRate_IsCustom = false;
                    Custom_BaudRate_Value = null;

                    Selected_Parity = null;

                    Selected_DataBits = null;

                    Selected_StopBits = null;

                    return;
                }

                Selected_BaudRate = SettingsFile.Settings.Connection_SerialPort.BaudRate;
                BaudRate_IsCustom = SettingsFile.Settings.Connection_SerialPort.BaudRate_IsCustom;
                Custom_BaudRate_Value = SettingsFile.Settings.Connection_SerialPort.BaudRate_Custom;

                Selected_Parity = SettingsFile.Settings.Connection_SerialPort.Parity;

                Selected_DataBits = SettingsFile.Settings.Connection_SerialPort.DataBits;

                Selected_StopBits = SettingsFile.Settings.Connection_SerialPort.StopBits;
            }

            catch (Exception error)
            {
                Message.Invoke("Ошибка обновления значений на странице SerialPort.\n\n" + error.Message, MessageType.Error);
            }
        }

        private void ReScan_SerialPorts(SerialPort_Info? info)
        {
            string[] portsList = SerialPort.GetPortNames();

            SerialPorts.Clear();

            foreach (string port in portsList)
            {
                SerialPorts.Add(port);
            }

            if (info == null || string.IsNullOrEmpty(info.Port))
            {
                Selected_SerialPort = null;
                Message_PortNotFound = "Порт не задан";
                Message_PortNotFound_IsVisible = true;

                return;
            }

            string selectedPort = info.Port;
            string? foundPort = null;

            foreach (string port in SerialPorts)
            {
                if (port == selectedPort)
                {
                    foundPort = port;
                    break;
                }
            }

            Selected_SerialPort = foundPort;

            if (string.IsNullOrEmpty(Selected_SerialPort))
            {
                Message_PortNotFound = selectedPort + " не найден";
                Message_PortNotFound_IsVisible = true;
            }

            else
            {
                Message_PortNotFound_IsVisible = false;
            }
        }

        protected override ValidateMessage? GetErrorMessage(string fieldName, string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (!StringValue.IsValidNumber(value, NumberStyles.Number, out uint _))
            {
                return AllErrorMessages[DecError_uint];
            }

            return null;
        }
    }
}
