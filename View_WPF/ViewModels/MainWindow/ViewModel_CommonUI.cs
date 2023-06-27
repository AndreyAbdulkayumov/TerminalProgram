using Core.Models;
using Core.Models.Http;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace View_WPF.ViewModels.MainWindow
{
    internal class ViewModel_CommonUI : ReactiveObject
    {
        public bool IsConnect
        {
            get => Model.HostIsConnect; 
        }

        public static string SettingsDocument
        {
            get
            {
                Properties.Settings.Default.Reload();
                return Properties.Settings.Default.SettingsDocument;
            }

            set
            {
                Properties.Settings.Default.SettingsDocument = value;
                Properties.Settings.Default.Save();
            }
        }

        private ObservableCollection<string> _presets = new ObservableCollection<string>();

        public ObservableCollection<string> Presets
        {
            get => _presets;
            set => this.RaiseAndSetIfChanged(ref _presets, value);
        }

        private string _selectedPreset = string.Empty;

        public string SelectedPreset
        {
            get => _selectedPreset;
            set => this.RaiseAndSetIfChanged(ref _selectedPreset, value);
        }

        public ReactiveCommand<Unit, Unit> Command_UpdatePresets { get; }

        public ReactiveCommand<Unit, Unit> Command_ProtocolMode_NoProtocol { get; }
        public ReactiveCommand<Unit, Unit> Command_ProtocolMode_Modbus { get; }

        public ReactiveCommand<Unit, Unit> Command_Connect { get; }
        public ReactiveCommand<Unit, Unit> Command_Disconnect { get; }
            

        private readonly ConnectedHost Model;

        private readonly Action<string, MessageType> Message;
        private readonly Action SetUI_Connected;
        private readonly Action SetUI_Disconnected;


        public ViewModel_CommonUI(
            Action<string, MessageType> MessageBox,
            Action UI_Connected_Handler,
            Action UI_Disconnected_Handler)
        {
            Message = MessageBox;
            SetUI_Connected = UI_Connected_Handler;
            SetUI_Disconnected = UI_Disconnected_Handler;

            Model = ConnectedHost.Model;

            SetUI_Disconnected.Invoke();

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;

            this.WhenAnyValue(x => x.SelectedPreset)
                .Where(x => x != null)
                .Where(x => x != string.Empty)
                .Subscribe(PresetName =>
                {
                    try
                    {
                        Model.ReadSettings(PresetName);
                        SettingsDocument = PresetName;
                    }

                    catch (Exception error)
                    {
                        Message.Invoke("Ошибка выбора пресета.\n\n" + error.Message, MessageType.Error);
                    }
                });

            Command_UpdatePresets = ReactiveCommand.Create(UpdateListOfPresets);
            Command_UpdatePresets.ThrownExceptions.Subscribe(error => Message.Invoke("Ошибка обновления списка пресетов.\n\n" + error.Message, MessageType.Error));

            Command_ProtocolMode_NoProtocol = ReactiveCommand.Create(Model.SetProtocol_NoProtocol);
            Command_ProtocolMode_NoProtocol.ThrownExceptions.Subscribe(error => Message.Invoke(error.Message, MessageType.Error));

            Command_ProtocolMode_Modbus = ReactiveCommand.Create(Model.SetProtocol_Modbus);
            Command_ProtocolMode_Modbus.ThrownExceptions.Subscribe(error => Message.Invoke(error.Message, MessageType.Error));

            Command_Connect = ReactiveCommand.Create(Connect_Handler);
            Command_Connect.ThrownExceptions.Subscribe(error => Message.Invoke(error.Message, MessageType.Error));

            Command_Disconnect = ReactiveCommand.CreateFromTask(Model.Disconnect);
            Command_Disconnect.ThrownExceptions.Subscribe(error => Message.Invoke(error.Message, MessageType.Error));
        }

        private void Model_DeviceIsConnect(object? sender, ConnectArgs e)
        {
            SetUI_Connected?.Invoke();
        }

        private void Model_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            SetUI_Disconnected?.Invoke();
        }

        private void UpdateListOfPresets()
        {
            string[] FileNames = Model.GetSettings_FileNames();

            Presets.Clear();

            foreach (string element in FileNames)
            {
                Presets.Add(element);
            }

            SelectedPreset = Presets.Single(x => x == SettingsDocument);
        }

        private void Connect_Handler()
        {
            DeviceData Settings = (DeviceData)Model.Settings.Clone();

            ConnectionInfo Info;

            switch (Settings.TypeOfConnection)
            {
                case DeviceData.ConnectionName_SerialPort:

                    Info = new ConnectionInfo(new SerialPortInfo(
                        Settings.Connection_SerialPort?.COMPort,
                        Settings.Connection_SerialPort?.BaudRate_IsCustom == true ?
                            Settings.Connection_SerialPort?.BaudRate_Custom : Settings.Connection_SerialPort?.BaudRate,
                        Settings.Connection_SerialPort?.Parity,
                        Settings.Connection_SerialPort?.DataBits,
                        Settings.Connection_SerialPort?.StopBits
                        ),
                        Model.GetEncoding(Settings.GlobalEncoding));

                    break;

                case DeviceData.ConnectionName_Ethernet:

                    Info = new ConnectionInfo(new SocketInfo(
                        Settings.Connection_IP?.IP_Address,
                        Settings.Connection_IP?.Port
                        ),
                        Model.GetEncoding(Settings.GlobalEncoding));

                    break;

                default:
                    throw new Exception("В файле настроек задан неизвестный интерфейс связи.");
            }

            Model.Connect(Info);
        }
    }
}
