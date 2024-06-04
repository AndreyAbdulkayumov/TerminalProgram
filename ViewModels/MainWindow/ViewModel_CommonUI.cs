using Core.Clients;
using Core.Models;
using Core.Models.Settings;
using ReactiveUI;
using MessageBox_Core;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;

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
        private bool ui_IsConnectedState = false;

        public bool UI_IsConnectedState
        {
            get => ui_IsConnectedState;
            set => this.RaiseAndSetIfChanged(ref ui_IsConnectedState, value);
        }

        private object? _currentViewModel;

        public object? CurrentViewModel
        {
            get => _currentViewModel;
            set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
        }

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

        private ObservableCollection<string> _presets = new ObservableCollection<string>();

        public ObservableCollection<string> Presets
        {
            get => _presets;
            set => this.RaiseAndSetIfChanged(ref _presets, value);
        }

        private string? _selectedPreset;

        public string? SelectedPreset
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

        // Нужно выставить true, а затем при инициализации false,
        // чтобы сработало выставление цвета после запуска программы
        // согласно заданной теме.
        private bool _led_TX_IsActive = true;

        public bool Led_TX_IsActive
        {
            get => _led_TX_IsActive;
            set => this.RaiseAndSetIfChanged(ref _led_TX_IsActive, value);
        }

        // Нужно выставить true, а затем при инициализации false,
        // чтобы сработало выставление цвета после запуска программы
        // согласно заданной теме.
        private bool _led_RX_IsActive = true;

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

        private readonly Action Set_Dark_Theme, Set_Light_Theme;

        private readonly ViewModel_NoProtocol NoProtocol_VM;
        private readonly ViewModel_ModbusClient ModbusClient_VM;


        public ViewModel_CommonUI(
            Func<Task> Open_ModbusScanner,
            Action<string, MessageType> MessageBox,
            Action Set_Dark_Theme_Handler,
            Action Set_Light_Theme_Handler,
            Func<string, Task> CopyToClipboard
            )
        {
            Model = ConnectedHost.Model;
            SettingsFile = Model_Settings.Model;

            Message = MessageBox;

            Set_Dark_Theme = Set_Dark_Theme_Handler;
            Set_Light_Theme = Set_Light_Theme_Handler;

            SettingsDocument = SettingsFile.AppData.SelectedPresetFileName;

            StringValue.ShowMessageView = Message;

            NoProtocol_VM = new ViewModel_NoProtocol(MessageBox);
            ModbusClient_VM = new ViewModel_ModbusClient(Open_ModbusScanner, MessageBox, CopyToClipboard);

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

                        ConnectionString = GetConnectionString();

                        if (SettingsFile.AppData.SelectedPresetFileName != PresetName)
                        {
                            SettingsDocument = PresetName;
                            SettingsFile.AppData.SelectedPresetFileName = PresetName;
                            SettingsFile.SaveAppInfo(SettingsFile.AppData);
                        }
                    }

                    catch (Exception error)
                    {
                        Message.Invoke("Ошибка выбора пресета.\n\n" + error.Message, MessageType.Error);
                    }
                });

            Command_UpdatePresets = ReactiveCommand.Create(UpdateListOfPresets);
            Command_UpdatePresets.ThrownExceptions.Subscribe(error => Message.Invoke("Ошибка обновления списка пресетов.\n\n" + error.Message, MessageType.Error));

            Command_ProtocolMode_NoProtocol = ReactiveCommand.Create(() => 
            {
                CurrentViewModel = NoProtocol_VM;
                Model.SetProtocol_NoProtocol();

                SettingsFile.AppData.SelectedMode = AppMode.NoProtocol;
                SettingsFile.SaveAppInfo(SettingsFile.AppData);
            });
            Command_ProtocolMode_NoProtocol.ThrownExceptions.Subscribe(error => Message.Invoke(error.Message, MessageType.Error));

            Command_ProtocolMode_Modbus = ReactiveCommand.Create(() =>
            {
                CurrentViewModel = ModbusClient_VM;
                Model.SetProtocol_Modbus();

                SettingsFile.AppData.SelectedMode = AppMode.ModbusClient;
                SettingsFile.SaveAppInfo(SettingsFile.AppData);
            });
            Command_ProtocolMode_Modbus.ThrownExceptions.Subscribe(error => Message.Invoke(error.Message, MessageType.Error));

            Command_Connect = ReactiveCommand.Create(Connect_Handler);
            Command_Connect.ThrownExceptions.Subscribe(error => Message.Invoke(error.Message, MessageType.Error));

            Command_Disconnect = ReactiveCommand.CreateFromTask(Model.Disconnect);
            Command_Disconnect.ThrownExceptions.Subscribe(error => Message.Invoke(error.Message, MessageType.Error));


            // Действия после запуска приложения

            SetAppTheme(SettingsFile.AppData.ThemeName);

            SetAppMode(SettingsFile.AppData.SelectedMode);
        }

        private void SetAppTheme(AppTheme ThemeName)
        {
            switch (ThemeName)
            {
                case AppTheme.Dark:
                    Set_Dark_Theme?.Invoke();
                    break;

                case AppTheme.Light:
                    Set_Light_Theme?.Invoke();
                    break;

                default:
                    Set_Dark_Theme?.Invoke();

                    SettingsFile.AppData.ThemeName = AppTheme.Dark;
                    SettingsFile.SaveAppInfo(SettingsFile.AppData);
                    break;
            }
        }

        private void SetAppMode(AppMode Mode)
        {
            switch (Mode)
            {
                case AppMode.NoProtocol:
                    CurrentViewModel = NoProtocol_VM;
                    break;

                case AppMode.ModbusClient:
                    CurrentViewModel = ModbusClient_VM;
                    break;

                default:
                    CurrentViewModel = NoProtocol_VM;
                    break;
            }
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
                        if ((Settings.Connection_SerialPort.BaudRate_IsCustom == false &&
                             (Settings.Connection_SerialPort.BaudRate == null || Settings.Connection_SerialPort.BaudRate == String.Empty)) ||
                            (Settings.Connection_SerialPort.BaudRate_IsCustom == true &&
                             (Settings.Connection_SerialPort.BaudRate_Custom == null || Settings.Connection_SerialPort.BaudRate_Custom == String.Empty)) ||
                            Settings.Connection_SerialPort.Parity == null || Settings.Connection_SerialPort.Parity == String.Empty ||
                            Settings.Connection_SerialPort.DataBits == null || Settings.Connection_SerialPort.DataBits == String.Empty ||
                            Settings.Connection_SerialPort.StopBits == null || Settings.Connection_SerialPort.StopBits == String.Empty)
                        {
                            ConnectionString = "Не заданы настройки для последовательного порта";
                        }

                        else
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
            if (Model.Client != null)
            {
                Model.Client.Notifications.TX_Notification += Notifications_TX_Notification;
                Model.Client.Notifications.RX_Notification += Notifications_RX_Notification;

                // Использовать для демонстрации работы уведомлений приема и передачи.
                //Task.Run(Model.Client.Notifications.DemoVisualization);
            }         

            UI_IsConnectedState = true;

            ConnectionStatus = ConnectionStatus_Connected;

            ConnectionTimer_IsVisible = true;

            ConnectionTime = new TimeSpan();

            ConnectionTimer_View = string.Format("{0:D2} : {1:D2} : {2:D2}",
                ConnectionTime.Days * 24 + ConnectionTime.Hours,
                ConnectionTime.Minutes,
                ConnectionTime.Seconds);

            ConnectionTimer.Start();
        }

        private void Notifications_TX_Notification(object? sender, NotificationArgs e)
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

        private void Notifications_RX_Notification(object? sender, NotificationArgs e)
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

        private void Model_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            ConnectionTimer.Stop();

            if (Model.Client != null)
            {
                Model.Client.Notifications.TX_Notification -= Notifications_TX_Notification;
                Model.Client.Notifications.RX_Notification -= Notifications_RX_Notification;
            }

            UI_IsConnectedState = false;

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
                SettingsDocument = Presets.First();
            }

            SelectedPreset = Presets.Single(x => x == SettingsDocument);

            Led_TX_IsActive = false;
            Led_RX_IsActive = false;
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
