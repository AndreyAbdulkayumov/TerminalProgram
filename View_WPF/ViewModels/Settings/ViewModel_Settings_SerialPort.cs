using Core.Models;
using ReactiveUI;
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

namespace View_WPF.ViewModels.Settings
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

        private string _selected_BaudRate = String.Empty;

        public string Selected_BaudRate
        {
            get => _selected_BaudRate;
            set => this.RaiseAndSetIfChanged(ref  _selected_BaudRate, value);
        }

        private bool _selected_Custom_BaudRate = false;

        public bool Selected_Custom_BaudRate
        {
            get => _selected_Custom_BaudRate;
            set => this.RaiseAndSetIfChanged(ref _selected_Custom_BaudRate, value);
        }

        private string _custom_BaudRate_Value = String.Empty;

        public string Custom_BaudRate_Value
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

        private string _selected_Parity = String.Empty;

        public string Selected_Parity
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

        private string _selected_DataBits = String.Empty;

        public string Selected_DataBits
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

        private string _selected_StopBits = String.Empty;

        public string Selected_StopBits
        {
            get => _selected_StopBits;
            set => this.RaiseAndSetIfChanged(ref _selected_StopBits, value);
        }

        public ReactiveCommand<Unit, Unit> Command_ReScan_COMPorts { get; }


        private Action<string, MessageType> Message;

        private readonly ConnectedHost Model;

        public ViewModel_Settings_SerialPort(ViewModel_Settings Main_VM, Action<string, MessageType> MessageBox)
        {
            Main_VM.SettingsFileChanged += Main_VM_SettingsFileChanged;

            Message = MessageBox;

            Model = ConnectedHost.Model;

            Command_ReScan_COMPorts = ReactiveCommand.Create(ReScan_COMPorts);

            Command_ReScan_COMPorts.ThrownExceptions.Subscribe(error => Message?.Invoke(error.Message, MessageType.Error));

            this.WhenAnyValue(x => x.Custom_BaudRate_Value)
                .Where(x => x != string.Empty)
                .Select(Main_VM.CheckNumber)
                .Subscribe(result => Custom_BaudRate_Value = result);
        }

        private void Main_VM_SettingsFileChanged(object? sender, EventArgs e)
        {
            ReScan_COMPorts();

            Selected_BaudRate = Model.Settings.Connection_SerialPort.BaudRate;
            Selected_Custom_BaudRate = Model.Settings.Connection_SerialPort.BaudRate_IsCustom == "Enable" ? true : false;
            Custom_BaudRate_Value = Model.Settings.Connection_SerialPort.BaudRate_Custom;

            Selected_Parity = Model.Settings.Connection_SerialPort.Parity;

            Selected_DataBits = Model.Settings.Connection_SerialPort.DataBits;

            Selected_StopBits = Model.Settings.Connection_SerialPort.StopBits;
        }

        private void ReScan_COMPorts()
        {
            string[] PortsList = SerialPort.GetPortNames();

            COM_Ports.Clear();

            foreach (string Port in PortsList)
            {
                COM_Ports.Add(Port);
            }

            if (Model.Settings.Connection_SerialPort == null || Model.Settings.Connection_SerialPort.COMPort == null)
            {
                Selected_COM_Port = String.Empty;
                Message_PortNotFound = "Порт не задан";
                Message_PortNotFound_IsVisible = true;

                return;
            }

            string SelectedPort = Model.Settings.Connection_SerialPort.COMPort;
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
