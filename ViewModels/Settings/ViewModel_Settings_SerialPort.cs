using Core.Models.Settings;
using ReactiveUI;
using MessageBox_Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ViewModels.Settings
{
    public class ViewModel_Settings_SerialPort : ReactiveObject
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

        private ObservableCollection<string> _com_Ports = new ObservableCollection<string>();

        public ObservableCollection<string> COM_Ports
        {
            get => _com_Ports;
            set => this.RaiseAndSetIfChanged(ref _com_Ports, value);
        }

        private string _selected_COM_Port = String.Empty;

        public string Selected_COM_Port
        {
            get => _selected_COM_Port;
            set => this.RaiseAndSetIfChanged(ref _selected_COM_Port, value);
        }

        /************************************/
        //
        //  BaudRate
        //
        /************************************/

        private ObservableCollection<string> _baudRate = new ObservableCollection<string>()
        {
            "4800", "9600", "19200", "38400", "57600", "115200"
        };

        public ObservableCollection<string> BaudRate
        {
            get => _baudRate;
        }

        private string? _selected_BaudRate = String.Empty;

        public string? Selected_BaudRate
        {
            get => _selected_BaudRate;
            set => this.RaiseAndSetIfChanged(ref  _selected_BaudRate, value);
        }

        private bool _baudRate_IsCustom = false;

        public bool BaudRate_IsCustom
        {
            get => _baudRate_IsCustom;
            set => this.RaiseAndSetIfChanged(ref _baudRate_IsCustom, value);
        }

        private string? _custom_BaudRate_Value = String.Empty;

        public string? Custom_BaudRate_Value
        {
            get => _custom_BaudRate_Value;
            set => this.RaiseAndSetIfChanged(ref _custom_BaudRate_Value, value);
        }

        /************************************/
        //
        //  Parity
        //
        /************************************/

        private ObservableCollection<string> _parity = new ObservableCollection<string>()
        {
            "None", "Even", "Odd"
        };

        public ObservableCollection<string> Parity 
        { 
            get => _parity; 
        }

        private string? _selected_Parity = String.Empty;

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

        private ObservableCollection<string> _dataBits = new ObservableCollection<string>()
        {
            "5", "6", "7", "8"
        };

        public ObservableCollection<string> DataBits
        {
            get => _dataBits;
        }

        private string? _selected_DataBits = String.Empty;

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

        private ObservableCollection<string> _stopBits = new ObservableCollection<string>()
        {
            "0", "1", "1.5", "2"
        };

        public ObservableCollection<string> StopBits
        {
            get => _stopBits;
        }

        private string? _selected_StopBits = String.Empty;

        public string? Selected_StopBits
        {
            get => _selected_StopBits;
            set => this.RaiseAndSetIfChanged(ref _selected_StopBits, value);
        }

        public ReactiveCommand<Unit, Unit> Command_ReScan_COMPorts { get; }


        private readonly Action<string, MessageType> Message;

        private readonly Model_Settings SettingsFile;


        public ViewModel_Settings_SerialPort(ViewModel_Settings Main_VM)
        {
            Main_VM.SettingsFileChanged += Main_VM_SettingsFileChanged;

            Message = Main_VM.Message;

            SettingsFile = Model_Settings.Model;

            Command_ReScan_COMPorts = ReactiveCommand.Create(() =>
            {
                if (SettingsFile.Settings == null)
                {
                    throw new Exception("Не инициализирован файл настроек.");
                }

                if (SettingsFile.Settings.Connection_SerialPort == null)
                {
                    throw new Exception("Не инициализирован файл настроек.");
                }

                ReScan_COMPorts(SettingsFile.Settings.Connection_SerialPort);
            });

            Command_ReScan_COMPorts.ThrownExceptions.Subscribe(error => Message.Invoke(error.Message, MessageType.Error));

            this.WhenAnyValue(x => x.Custom_BaudRate_Value)
                .WhereNotNull()
                .Where(x => x != string.Empty)
                .Select(x => StringValue.CheckNumber(x, System.Globalization.NumberStyles.Number, out UInt32 _))
                .Subscribe(result => Custom_BaudRate_Value = result);
        }

        private void Main_VM_SettingsFileChanged(object? sender, EventArgs e)
        {
            try
            {
                if (SettingsFile.Settings == null)
                {
                    throw new Exception("Не инициализирован файл настроек.");
                }

                if (SettingsFile.Settings.Connection_SerialPort == null)
                {
                    return;
                }

                ReScan_COMPorts(SettingsFile.Settings.Connection_SerialPort);

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

        private void ReScan_COMPorts(SerialPort_Info Info)
        {
            string[] PortsList = SerialPort.GetPortNames();

            COM_Ports.Clear();

            foreach (string Port in PortsList)
            {
                COM_Ports.Add(Port);
            }

            if (Info.COMPort == null || Info.COMPort == String.Empty)
            {
                Selected_COM_Port = String.Empty;
                Message_PortNotFound = "Порт не задан";
                Message_PortNotFound_IsVisible = true;

                return;
            }

            string SelectedPort = Info.COMPort;
            string? FoundPort = null;

            foreach (string Port in COM_Ports)
            {
                if (Port == SelectedPort)
                {
                    FoundPort = Port;
                    break;
                }
            }

            if (FoundPort == null)
            {
                Selected_COM_Port = String.Empty;

                Message_PortNotFound = "Порт " + SelectedPort + " не найден";
                Message_PortNotFound_IsVisible = true;
            }

            else
            {
                Selected_COM_Port = FoundPort;
                Message_PortNotFound_IsVisible = false;
            }
        }
    }
}
