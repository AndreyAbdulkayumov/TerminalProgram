using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using ReactiveUI;
using View_WPF.Properties;

namespace View_WPF.ViewModels.Settings
{
    public class ViewModel_Settings : ReactiveObject
    {
        private ObservableCollection<string> _devices = new ObservableCollection<string>();

        public ObservableCollection<string> Devices
        {
            get => _devices;
            set => this.RaiseAndSetIfChanged(ref _devices, value);
        }

        private string _selectedDevice = string.Empty;

        public string SelectedDevice
        {
            get => _selectedDevice;
            set => this.RaiseAndSetIfChanged(ref _selectedDevice, value);
        }

        private readonly ObservableCollection<string> _typeOfEncoding = new ObservableCollection<string>()
        {
            "ASCII", "UTF-8", "UTF-32", "Unicode"
        };

        public ObservableCollection<string> TypeOfEncoding
        {
            get => _typeOfEncoding;
        }

        private string _selectedEncoding = string.Empty;

        public string SelectedEncoding
        {
            get => _selectedEncoding;
            set => this.RaiseAndSetIfChanged(ref _selectedEncoding, value);
        }

        private string _writeTimeout = string.Empty;

        public string WriteTimeout
        {
            get => _writeTimeout;
            set => this.RaiseAndSetIfChanged(ref _writeTimeout, value);
        }

        private string _readTimeout = string.Empty;

        public string ReadTimeout
        {
            get => _readTimeout;
            set => this.RaiseAndSetIfChanged(ref _readTimeout, value);
        }

        private bool _selected_SerialPort;

        public bool Selected_SerialPort
        {
            get => _selected_SerialPort;
            set => this.RaiseAndSetIfChanged(ref _selected_SerialPort, value);
        }

        private bool _selected_Ethernet;

        public bool Selected_Ethernet
        {
            get => _selected_Ethernet;
            set => this.RaiseAndSetIfChanged(ref _selected_Ethernet, value);
        }

        
        public ReactiveCommand<Unit, Unit> Command_Loaded { get; }


        private Action<string, MessageType> Message;

        private readonly ConnectedHost Model;

        public event EventHandler<EventArgs>? SettingsFileChanged;

        public readonly ViewModel_Settings_Ethernet Ethernet_VM;
        public readonly ViewModel_Settings_SerialPort SerialPort_VM;

        public ViewModel_Settings(Action<string, MessageType> MessageBox)
        {
            Message = MessageBox;

            Model = ConnectedHost.Model;

            Command_Loaded = ReactiveCommand.CreateFromTask(Loaded_EventHandler);

            this.WhenAnyValue(x => x.SelectedDevice)
                .Where(x => x != string.Empty)
                .Select(async value => await Model.ReadSettings(value))
                .Subscribe(UpdateUI);

            Ethernet_VM = new ViewModel_Settings_Ethernet(this, MessageBox);
            SerialPort_VM = new ViewModel_Settings_SerialPort(this, MessageBox);
        }

        private async void UpdateUI(Task task)
        {
            await task;

            DeviceData Settings = Model.Settings;

            SelectedEncoding = Settings.GlobalEncoding ?? string.Empty;

            WriteTimeout = Settings.TimeoutWrite ?? string.Empty;
            ReadTimeout = Settings.TimeoutRead ?? string.Empty;

            switch (Settings.TypeOfConnection)
            {
                case "SerialPort":
                    Selected_SerialPort = true;
                    break;

                case "Ethernet":
                    Selected_Ethernet = true;
                    break;
            }

            SettingsFileChanged?.Invoke(this, new EventArgs());
        }

        private async Task Loaded_EventHandler()
        {
            string[] FileNames = await Model.GetSettings_FileNames();

            Devices.Clear();

            foreach (string element in FileNames)
            {
                Devices.Add(element);
            }

            SelectedDevice = Devices.First();
        }
    }
}
