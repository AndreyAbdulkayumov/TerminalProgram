using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.IO.Ports;
using SystemOfSaving;
using TerminalProgram.ServiceWindows;
using TerminalProgram.Settings;
using TerminalProgram.Protocols;
using TerminalProgram.Protocols.NoProtocol;
using TerminalProgram.Protocols.Modbus;
using TerminalProgram.Protocols.Http;

namespace TerminalProgram
{
    public enum ProgramDirectory
    {
        Settings
    }

    public static class UsedDirectories
    {
        private readonly static string Path_Settings = "Settings/";

        public static string GetPath(ProgramDirectory Type)
        {
            switch (Type)
            {
                case ProgramDirectory.Settings:
                    return Path_Settings;

                default:
                    throw new Exception("Выбрана неизвестный тип директории.");
            }
        }
    }

    public class ConnectArgs : EventArgs
    {
        public IConnection ConnectedDevice;

        public ConnectArgs(IConnection ConnectedDevice)
        {
            this.ConnectedDevice = ConnectedDevice;
        }
    }

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public event EventHandler<ConnectArgs> DeviceIsConnect;
        public event EventHandler<ConnectArgs> DeviceIsDisconnected;

        public DeviceData Settings { get; private set; } = new DeviceData();

        public static Encoding GlobalEncoding { get; private set; } = null;

        private IConnection Client = null;

        private readonly SettingsMediator SettingsManager = new SettingsMediator();
        
        private string[] PresetFileNames;

        private UI_NoProtocol NoProtocolPage = null;
        private UI_Modbus ModbusPage = null;
        private UI_Http HttpPage = null;

        public ProtocolMode SelectedProtocol { get; private set; } = null;

        private string SettingsDocument
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

        public MainWindow()
        {
            InitializeComponent();
        }

        public static string[] GetDeviceList()
        {
            string[] Devices = Directory.GetFiles(UsedDirectories.GetPath(ProgramDirectory.Settings));

            for (int i = 0; i < Devices.Length; i++)
            {
                Devices[i] = System.IO.Path.GetFileNameWithoutExtension(Devices[i]);
            }

            return Devices;
        }

        private void SourceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Check.FilesDirectory();

                SystemOfPresets.FindFilesOfPresets(ref PresetFileNames);

                for (int i = 0; i < PresetFileNames.Length; i++)
                {
                    ComboBox_SelectedPreset.Items.Add(PresetFileNames[i]);
                }

                if (Check.SettingsFile(ref PresetFileNames, SettingsDocument) == false)
                {
                    MessageBox.Show("Файл настроек не существует в папке " + UsedDirectories.GetPath(ProgramDirectory.Settings) +
                        "\n\nНажмите ОК и выберите один из доступных файлов в появившемся окне.",
                        this.Title, MessageBoxButton.OK,
                        MessageBoxImage.Error, MessageBoxResult.OK);

                    ComboBoxWindow window = new ComboBoxWindow(ref PresetFileNames)
                    {
                        Owner = this
                    };

                    window.ShowDialog();

                    if (window.SelectedDocumentPath != String.Empty)
                    {
                        SettingsDocument = window.SelectedDocumentPath;
                    }

                    else
                    {
                        Application.Current.Shutdown();
                        return;
                    }
                }

                SetUI_Disconnected();

                UpdateDeviceData(SettingsDocument);

                NoProtocolPage = new UI_NoProtocol(this)
                {
                    Height = Grid_Action.ActualHeight,
                    Width = Grid_Action.ActualWidth,

                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };

                NoProtocolPage.ErrorHandler += CommonErrorHandler;

                ModbusPage = new UI_Modbus(this)
                {
                    Height = Grid_Action.ActualHeight,
                    Width = Grid_Action.ActualWidth,

                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };

                ModbusPage.ErrorHandler += CommonErrorHandler;

                HttpPage = new UI_Http(this)
                {
                    Height = Grid_Action.ActualHeight,
                    Width = Grid_Action.ActualWidth,

                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };

                RadioButton_NoProtocol.IsChecked = true;
            }

            catch (Exception error)
            {
                MessageBox.Show("Ошибка инициализации: \n\n" + error.Message + "\n\nПриложение будет закрыто.", this.Title,
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                Application.Current.Shutdown();
            }
        }

        private void CommonErrorHandler(object sender, EventArgs e)
        {
            Button_Disconnect_Click(this, new RoutedEventArgs());
        }

        private async void SourceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (Client != null && Client.IsConnected)
                {
                    if (MessageBox.Show("Клиент ещё подключен к хосту.\nЗакрыть программу?", this.Title,
                        MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }

                    await Client.Disconnect();
                }
            }
            
            catch(Exception error)
            {
                MessageBox.Show("Возникла ошибка во время закрытия программы.\n\n" + error.Message, this.Title,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateDeviceData(string DocumentName)
        {
            try
            {
                SettingsManager.LoadSettingsFrom(
                    UsedDirectories.GetPath(ProgramDirectory.Settings) +
                    DocumentName + 
                    SettingsManager.FileType
                    );

                List<string> Devices = SettingsManager.GetAllDevicesNames();

                if (Devices.Count > 0)
                {
                    Settings = SettingsManager.GetDeviceData(Devices[0]);

                    ComboBox_SelectedPreset.SelectedIndex = ComboBox_SelectedPreset.Items.IndexOf(DocumentName);

                    GlobalEncoding = GetEncoding(Settings.GlobalEncoding);
                }

                else
                {
                    MessageBox.Show("В документе " + UsedDirectories.GetPath(ProgramDirectory.Settings) + DocumentName +
                        ".xml" + " нет настроек устройства. Создайте их в меню Настройки.", this.Title, MessageBoxButton.OK,
                        MessageBoxImage.Error, MessageBoxResult.OK);

                    ComboBox_SelectedPreset.SelectedIndex = -1;
                }
            }

            catch (Exception error)
            {
                MessageBox.Show("Ошибка чтения данных из документа." +
                    " Проверьте его целостность или выберите другой файл настроек." +
                    " Возможно данный файл не совместим с текущей версией программы.\n\n" + error.Message,
                    this.Title, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                return;
            }
        }

        private Encoding GetEncoding(string EncodingName)
        {
            switch (EncodingName)
            {
                case "ASCII":
                    return Encoding.ASCII;

                case "Unicode":
                    return Encoding.Unicode;

                case "UTF-32":
                    return Encoding.UTF32;

                case "UTF-7":
                    return Encoding.UTF7;

                case "UTF-8":
                    return Encoding.UTF8;

                default:
                    throw new Exception("Задан неизвестный тип кодировки: " + EncodingName);
            }
        }

        private void MenuSettings_Click(object sender, RoutedEventArgs e)
        {
            if (PresetFileNames == null || PresetFileNames.Length == 0)
            {
                MessageBox.Show("Не найдено ни одно файла настроек.", this.Title,
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                return;
            }

            if (SettingsDocument == String.Empty)
            {
                MessageBox.Show("Не выбран файл настроек", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                return;
            }

            SettingsWindow Window = new SettingsWindow(
                UsedDirectories.GetPath(ProgramDirectory.Settings),
                SettingsDocument,
                SettingsManager
                )
            {
                Owner = this
            };

            Window.ShowDialog();

            string[] Devices = MainWindow.GetDeviceList();

            ComboBox_SelectedPreset.Items.Clear();

            bool SettingDocumentExists = false;

            for(int i = 0; i < Devices.Length; i++)
            {
                ComboBox_SelectedPreset.Items.Add(Devices[i]);
                
                if (Devices[i] == SettingsDocument)
                {
                    SettingDocumentExists = true;
                }
            }

            if (Window.SettingsIsChanged)
            {
                SettingsDocument = Window.SettingsDocument;
            }

            else if (SettingDocumentExists == false && Devices.Length != 0)
            {
                SettingsDocument = Devices[0];
            }

            UpdateDeviceData(SettingsDocument);
        }

        private void ComboBox_SelectedPreset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_SelectedPreset.SelectedItem != null)
            {
                SettingsDocument = ComboBox_SelectedPreset.SelectedItem.ToString();
                UpdateDeviceData(SettingsDocument);
            }
        }

        private void Button_Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (Settings.TypeOfConnection)
                {
                    case "SerialPort":

                        Client = new SerialPortClient();

                        Client.Connect(new ConnectionInfo(new SerialPortInfo(
                            Settings.COMPort,
                            Settings.BaudRate_IsCustom == "Enable" ? Settings.BaudRate_Custom : Settings.BaudRate,
                            Settings.Parity,
                            Settings.DataBits,
                            Settings.StopBits
                            ),
                            GlobalEncoding));

                        break;

                    case "Ethernet":

                        Client = new IPClient();

                        Client.Connect(new ConnectionInfo(new SocketInfo(
                            Settings.IP,
                            Settings.Port
                            ),
                            GlobalEncoding));

                        break;

                    default:
                        throw new Exception("В файле настроек задан неизвестный интерфейс связи.");
                }

                SetUI_Connected();

                if (SelectedProtocol is ProtocolMode_Modbus)
                {
                    (SelectedProtocol as ProtocolMode_Modbus).UpdateTimeouts(Settings);
                }

                SelectedProtocol.InitMode(Client);

                if (DeviceIsConnect != null)
                {
                    DeviceIsConnect(this, new ConnectArgs(Client));
                }
            }
            
            catch(Exception error)
            {
                MessageBox.Show("Возникла ошибка при подключении к устройству.\n\n" + error.Message, this.Title,
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        private async void Button_Disconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    await Client.Disconnect();
                }

                catch (Exception error)
                {
                    MessageBox.Show("Возникла ошибка при попытке отключения от устройства:\n" + error.Message, this.Title,
                        MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                }

                SetUI_Disconnected();

                if (DeviceIsDisconnected != null)
                {
                    DeviceIsDisconnected(this, new ConnectArgs(Client));
                }
            }

            catch(Exception error)
            {
                MessageBox.Show("Возникла ошибка при нажатии на кнопку \"Отключить\":\n" + error.Message, this.Title,
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        private void SetUI_Connected()
        {
            Button_Connect.IsEnabled = false;
            Button_Disconnect.IsEnabled = true;

            MenuSettings.IsEnabled = false;
        }

        private void SetUI_Disconnected()
        {
            Button_Connect.IsEnabled = true;
            Button_Disconnect.IsEnabled = false;

            MenuSettings.IsEnabled = true;
        }

        private void RadioButton_NoProtocol_Checked(object sender, RoutedEventArgs e)
        {
            if (Frame_ActionUI.Navigate(NoProtocolPage) == false)
            {
                MessageBox.Show("Не удалось перейти на страницу " + NoProtocolPage.Name, this.Title,
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                return;
            }

            GridRow_Header.Height = new GridLength(100);
            TextBlock_SelectedPreset.Visibility = Visibility.Visible;
            ComboBox_SelectedPreset.Visibility = Visibility.Visible;
            Button_Connect.Visibility = Visibility.Visible;
            Button_Disconnect.Visibility = Visibility.Visible;

            SelectedProtocol = new ProtocolMode_NoProtocol(Client);
        }

        private void RadioButton_Protocol_Modbus_Checked(object sender, RoutedEventArgs e)
        {
            if (Frame_ActionUI.Navigate(ModbusPage) == false)
            {
                MessageBox.Show("Не удалось перейти на страницу " + ModbusPage.Name, this.Title,
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                return;
            }

            GridRow_Header.Height = new GridLength(100);
            TextBlock_SelectedPreset.Visibility = Visibility.Visible;
            ComboBox_SelectedPreset.Visibility = Visibility.Visible;
            Button_Connect.Visibility = Visibility.Visible;
            Button_Disconnect.Visibility = Visibility.Visible;

            SelectedProtocol = new ProtocolMode_Modbus(Client, Settings);
        }

        private void RadioButton_Protocol_Http_Checked(object sender, RoutedEventArgs e)
        {
            if (Frame_ActionUI.Navigate(HttpPage) == false)
            {
                MessageBox.Show("Не удалось перейти на страницу " + HttpPage.Name, this.Title,
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                return;
            }

            GridRow_Header.Height = new GridLength(50);
            TextBlock_SelectedPreset.Visibility = Visibility.Hidden;
            ComboBox_SelectedPreset.Visibility = Visibility.Hidden;
            Button_Connect.Visibility = Visibility.Hidden;
            Button_Disconnect.Visibility = Visibility.Hidden;
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow window = new AboutWindow()
            {
                Owner = this
            };

            window.ShowDialog();
        }
    }

    
}
