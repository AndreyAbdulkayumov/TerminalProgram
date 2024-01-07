using Core.Clients;
using Core.Models;
using Core.Models.Settings;
using ReactiveUI;
using MessageBox_Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ViewModels.MainWindow
{
    public class DocArgs : EventArgs
    {
        public readonly string? FilePath;

        public DocArgs(string? FilePath)
        {
            this.FilePath = FilePath;
        }
    }

    public class ViewModel_CommonUI : ReactiveObject
    {
        public bool IsConnected
        {
            get => Model.HostIsConnect;
        }

        public static event EventHandler<DocArgs>? SettingsDocument_Changed;

        private static string? _settingsDocument;

        public static string? SettingsDocument
        {
            get
            {
                return _settingsDocument;
            }

            set
            {
                _settingsDocument = value;
                SettingsDocument_Changed?.Invoke(null, new DocArgs(value));
            }
        }

        public static event EventHandler<DocArgs>? ThemeName_Changed;

        private static string? _themeName;

        public static string? ThemeName
        {
            get
            {
                return _themeName;
            }

            set
            {
                _themeName = value;
                ThemeName_Changed?.Invoke(null, new DocArgs(value));
            }
        }

        public static string? ThemeName_Dark;
        public static string? ThemeName_Light;

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


        private string? _connectionString;

        public string? ConnectionString
        {
            get => _connectionString;
            set => this.RaiseAndSetIfChanged(ref _connectionString, value);
        }

        private const string ConnectionStatus_Connected = "Подключено";
        private const string ConnectionStatus_Disconnected = "Отключено";

        private string _connectionStatus = ConnectionStatus_Disconnected;

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => this.RaiseAndSetIfChanged(ref _connectionStatus, value);
        }

        private bool _connectionTimer_IsVisible = false;

        public bool ConnectionTimer_IsVisible
        {
            get => _connectionTimer_IsVisible;
            set => this.RaiseAndSetIfChanged(ref _connectionTimer_IsVisible, value);
        }

        private string _connectionTimer_View = "";

        public string ConnectionTimer_View
        {
            get => _connectionTimer_View;
            set => this.RaiseAndSetIfChanged(ref _connectionTimer_View, value);
        }

        private const int ConnectionTimer_Interval_ms = 1000;
        private readonly System.Timers.Timer ConnectionTimer;

        private TimeSpan ConnectionTime = new TimeSpan();

        private bool _led_TX_IsActive;

        public bool Led_TX_IsActive
        {
            get => _led_TX_IsActive;
            set => this.RaiseAndSetIfChanged(ref _led_TX_IsActive, value);
        }

        private bool _led_RX_IsActive;

        public bool Led_RX_IsActive
        {
            get => _led_RX_IsActive;
            set => this.RaiseAndSetIfChanged(ref _led_RX_IsActive, value);
        }

        private object TX_View_Locker = new object();
        private object RX_View_Locker = new object();

        private readonly ConnectedHost Model;
        private readonly Model_Settings SettingsFile;

        private readonly Action<string, MessageType> Message;
        private readonly Action SetUI_Connected;
        private readonly Action SetUI_Disconnected;
        private readonly Func<string[], string?> Select_AvailablePresetFile;

        private INotifications_TX_RX? TX_RX_Notification;

        public ViewModel_CommonUI(
            Action<string, MessageType> MessageBox,
            Action UI_Connected_Handler,
            Action UI_Disconnected_Handler,
            Func<string[], string?> Select_AvailablePresetFile_Handler,
            string? SettingsDocument,
            string? CurrentThemeName,
            string? ThemeName_Dark,
            string? ThemeName_Light
            )
        {
            Message = MessageBox;
            SetUI_Connected = UI_Connected_Handler;
            SetUI_Disconnected = UI_Disconnected_Handler;
            Select_AvailablePresetFile = Select_AvailablePresetFile_Handler;
            ViewModel_CommonUI.SettingsDocument = SettingsDocument;
            ViewModel_CommonUI.ThemeName = CurrentThemeName;
            ViewModel_CommonUI.ThemeName_Dark = ThemeName_Dark;
            ViewModel_CommonUI.ThemeName_Light = ThemeName_Light;

            Model = ConnectedHost.Model;
            SettingsFile = Model_Settings.Model;

            StringValue.ShowMessageView = Message;

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;

            ConnectionTimer = new System.Timers.Timer(ConnectionTimer_Interval_ms);

            ConnectionTimer.Elapsed += ConnectionTimer_Elapsed;

            this.WhenAnyValue(x => x.SelectedPreset)
                .WhereNotNull()
                .Where(x => x != string.Empty)
                .Subscribe(PresetName =>
                {
                    try
                    {
                        SettingsFile.Read(PresetName);
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


            // Действия после запуска приложения

            SetUI_Disconnected.Invoke();
        }

        private void ConnectionTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            ConnectionTime = ConnectionTime.Add(new TimeSpan(0, 0, 0, 0, ConnectionTimer_Interval_ms));

            ConnectionTimer_View = string.Format("{0:D2} : {1:D2} : {2:D2}", 
                ConnectionTime.Days * 24 + ConnectionTime.Hours, 
                ConnectionTime.Minutes, 
                ConnectionTime.Seconds);
        }

        private void Model_DeviceIsConnect(object? sender, ConnectArgs e)
        {
            TX_RX_Notification = Model.Client as INotifications_TX_RX;

            if (TX_RX_Notification != null)
            {
                TX_RX_Notification.TX_Notification += TX_RX_Notification_TX_Notification;
                TX_RX_Notification.RX_Notification += TX_RX_Notification_RX_Notification;
            }

            SetUI_Connected.Invoke();

            ConnectionStatus = ConnectionStatus_Connected;

            ConnectionTimer_IsVisible = true;

            ConnectionTime = new TimeSpan();

            ConnectionTimer_View = string.Format("{0:D2} : {1:D2} : {2:D2}",
                ConnectionTime.Days * 24 + ConnectionTime.Hours,
                ConnectionTime.Minutes,
                ConnectionTime.Seconds);

            ConnectionTimer.Start();
        }

        private void TX_RX_Notification_RX_Notification(object? sender, NotificationArgs e)
        {
            lock (RX_View_Locker)
            {
                if (e.IsStarted)
                {
                    Led_RX_IsActive = true;
                    return;
                }

                Led_RX_IsActive = false;
            }
        }        

        private void TX_RX_Notification_TX_Notification(object? sender, NotificationArgs e)
        {
            lock (TX_View_Locker)
            {
                if (e.IsStarted)
                {
                    Led_TX_IsActive = true;
                    return;
                }

                Led_TX_IsActive = false;
            }            
        }

        private void Model_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            ConnectionTimer.Stop();

            if (TX_RX_Notification != null)
            {
                TX_RX_Notification.TX_Notification -= TX_RX_Notification_TX_Notification;
                TX_RX_Notification.RX_Notification -= TX_RX_Notification_RX_Notification;
            }

            SetUI_Disconnected.Invoke();

            ConnectionStatus = ConnectionStatus_Disconnected;

            ConnectionTimer_IsVisible = false;
        }

        private void UpdateListOfPresets()
        {
            string[] FileNames = SettingsFile.FindFilesOfPresets();

            Presets.Clear();

            foreach (string element in FileNames)
            {
                Presets.Add(element);
            }

            if (SettingsDocument == null || Presets.Contains(SettingsDocument) == false)
            {
                Message.Invoke("Файл настроек \"" + SettingsDocument + "\" не существует в папке " + SettingsFile.FolderPath_Settings +
                    "\n\nНажмите ОК и выберите один из доступных файлов в появившемся окне.", MessageType.Warning);

                SettingsDocument = Select_AvailablePresetFile(Presets.ToArray());
            }

            if (SettingsDocument != null)
            {
                SelectedPreset = Presets.Single(x => x == SettingsDocument);
            }

            ConnectionString = GetConnectionString();
        }

        private string GetConnectionString()
        {
            if (SettingsFile.Settings == null)
            {
                throw new Exception("Настройки не инициализированы.");
            }

            DeviceData Settings = (DeviceData)SettingsFile.Settings.Clone();

            string Separator = " : ";

            string ConnectionString;

            switch (Settings.TypeOfConnection)
            {
                case DeviceData.ConnectionName_SerialPort:

                    if (Settings.Connection_SerialPort != null)
                    {
                        ConnectionString = 
                            (Settings.Connection_SerialPort.COMPort == null || Settings.Connection_SerialPort.COMPort == String.Empty ? 
                                "Порт не задан" : Settings.Connection_SerialPort.COMPort) +
                            Separator +
                            (Settings.Connection_SerialPort.BaudRate_IsCustom == true ? 
                                Settings.Connection_SerialPort.BaudRate_Custom : Settings.Connection_SerialPort.BaudRate) +
                            Separator +
                            Settings.Connection_SerialPort.Parity +
                            Separator +
                            Settings.Connection_SerialPort.DataBits +
                            Separator +
                            Settings.Connection_SerialPort.StopBits;
                    }

                    else
                    {
                        ConnectionString = "Настройки не заданы";
                    }
                    
                    break;

                case DeviceData.ConnectionName_Ethernet:

                    if (Settings.Connection_IP != null)
                    {
                        ConnectionString = 
                            (Settings.Connection_IP.IP_Address == null || Settings.Connection_IP.IP_Address == String.Empty ?
                                "IP адрес не задан" : Settings.Connection_IP.IP_Address) +
                            Separator +
                            (Settings.Connection_IP.Port == null || Settings.Connection_IP.Port == String.Empty ?
                                "Порт не задан" : Settings.Connection_IP.Port);
                    }

                    else
                    {
                        ConnectionString = "Настройки не заданы";
                    }
                    
                    break;

                default:
                    throw new Exception("Задан неизвестный тип подключения: " + Settings.TypeOfConnection);
            }

            return ConnectionString;
        }

        private void Connect_Handler()
        {
            if (SettingsFile.Settings == null)
            {
                throw new Exception("Настройки не инициализированы.");
            }

            DeviceData Settings = (DeviceData)SettingsFile.Settings.Clone();

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
                        GetEncoding(Settings.GlobalEncoding));

                    break;

                case DeviceData.ConnectionName_Ethernet:

                    Info = new ConnectionInfo(new SocketInfo(
                        Settings.Connection_IP?.IP_Address,
                        Settings.Connection_IP?.Port
                        ),
                        GetEncoding(Settings.GlobalEncoding));

                    break;

                default:
                    throw new Exception("В файле настроек задан неизвестный интерфейс связи.");
            }

            Model.Connect(Info);
        }

        public Encoding GetEncoding(string? EncodingName)
        {
            switch (EncodingName)
            {
                case "ASCII":
                    return Encoding.ASCII;

                case "Unicode":
                    return Encoding.Unicode;

                case "UTF-32":
                    return Encoding.UTF32;

                case "UTF-8":
                    return Encoding.UTF8;

                default:
                    throw new Exception("Задан неизвестный тип кодировки: " + EncodingName);
            }
        }
    }
}
