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
using TerminalProgram.Protocols;
using TerminalProgram.Protocols.NoProtocol;
using TerminalProgram.Protocols.Modbus;

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

        private IConnection Client = null;

        private readonly SettingsMediator SettingsManager = new SettingsMediator();
        private DeviceData Settings = new DeviceData();

        private string[] PresetFileNames;

        private UI_NoProtocol NoProtocolPage = null;
        private UI_Modbus ModbusPage = null;

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
                        "\n\nНажмите ОК и выберите один из доступных пресетов в появившемся окне.",
                        "Ошибка", MessageBoxButton.OK,
                        MessageBoxImage.Error, MessageBoxResult.OK);

                    Select window = new Select(ref PresetFileNames, "Выберите пресет");
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

                NoProtocolPage = new UI_NoProtocol(this)
                {
                    Height = Grid_Action.ActualHeight,
                    Width = Grid_Action.ActualWidth,

                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };

                ModbusPage = new UI_Modbus(this)
                {
                    Height = Grid_Action.ActualHeight,
                    Width = Grid_Action.ActualWidth,

                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };

                RadioButton_NoProtocol.IsChecked = true;

                UpdateDeviceData(SettingsDocument);
            }

            catch (Exception error)
            {
                MessageBox.Show("Ошибка инициализации: \n\n" + error.Message + "\n\nПриложение будет закрыто.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                Application.Current.Shutdown();
            }
        }
        
        private void UpdateDeviceData(string DocumentName)
        {
            try
            {
                SettingsManager.LoadSettingsFrom(UsedDirectories.GetPath(ProgramDirectory.Settings) + DocumentName + ".xml");

                List<string> Devices = SettingsManager.GetAllDevicesNames();

                if (Devices.Count > 0)
                {
                    Settings = SettingsManager.GetDeviceData(Devices[0]);

                    ComboBox_SelectedPreset.SelectedIndex = ComboBox_SelectedPreset.Items.IndexOf(DocumentName);
                }

                else
                {
                    MessageBox.Show("В документе " + UsedDirectories.GetPath(ProgramDirectory.Settings) + DocumentName +
                        ".xml" + " нет настроек устройства. Создайте их в меню Настройки.", "Предупреждение", MessageBoxButton.OK,
                        MessageBoxImage.Error, MessageBoxResult.OK);

                    ComboBox_SelectedPreset.SelectedIndex = -1;
                }
            }

            catch (Exception error)
            {
                MessageBox.Show("Ошибка чтения данных из документа. Проверьте его целостность или выберите другой файл настроек.\n\n" + error.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                return;
            }
        }

        private void MenuSettings_Click(object sender, RoutedEventArgs e)
        {
            if (PresetFileNames == null || PresetFileNames.Length == 0)
            {
                MessageBox.Show("Не найдено ни одно файла настроек.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }

            SettingsWindow Window = new SettingsWindow(UsedDirectories.GetPath(ProgramDirectory.Settings), ref PresetFileNames)
            {
                Owner = this
            };

            Window.ShowDialog();

            if (Window.SettingsIsChanged)
            {
                SettingsDocument = Window.SettingsDocument;
                UpdateDeviceData(SettingsDocument);
            }
        }

        private void ComboBox_SelectedPreset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_SelectedPreset.SelectedItem != null)
            {
                UpdateDeviceData(ComboBox_SelectedPreset.SelectedItem.ToString());
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
                            Settings.BaudRate,
                            Settings.Parity,
                            Settings.DataBits,
                            Settings.StopBits
                            )));

                        break;

                    case "Ethernet":

                        Client = new IPClient();

                        Client.Connect(new ConnectionInfo(new SocketInfo(
                            Settings.IP,
                            Settings.Port
                            )));

                        break;

                    default:
                        throw new Exception("В файле настроек задан неизвестный интерфейс связи.");
                }

                SetUI_Connected();

                if (DeviceIsConnect != null)
                {
                    DeviceIsConnect(this, new ConnectArgs(Client));
                }
            }
            
            catch(Exception error)
            {
                MessageBox.Show("Возникла ошибка при подключении к устройству.\n\n" + error.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK,
                    MessageBoxOptions.ServiceNotification);
            }
        }
         

        private void Button_Disconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Client.Disconnect();

                SetUI_Disconnected();

                if (DeviceIsDisconnected != null)
                {
                    DeviceIsDisconnected(this, new ConnectArgs(Client));
                }
            }

            catch(Exception error)
            {
                MessageBox.Show("Возникла ошибка при попытке отключения от устройства:\n" + error.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK,
                    MessageBoxOptions.ServiceNotification);
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
                throw new Exception("Не удалось перейти на страницу " + NoProtocolPage.Name);
            }
        }

        private void RadioButton_Protocol_Modbus_Checked(object sender, RoutedEventArgs e)
        {
            if (Frame_ActionUI.Navigate(ModbusPage) == false)
            {
                throw new Exception("Не удалось перейти на страницу " + ModbusPage.Name);
            }
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
