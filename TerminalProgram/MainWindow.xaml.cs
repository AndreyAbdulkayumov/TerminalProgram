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
using Communication;

namespace TerminalProgram
{
    public enum TypeOfMessage
    {
        Char,
        String
    };

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

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Connection Device = new Connection();

        private char[] BytesToSend;

        private readonly SettingsMediator SettingsManager = new SettingsMediator();
        private DeviceData Settings = new DeviceData();

        string[] PresetFiles;

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

        private TypeOfMessage MessageType;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SourceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetUI_Disconnected();

            RadioButton_Char.IsChecked = true;

            bool Error_SettingsDocument = false;
            

            try
            {
                if (Directory.Exists(UsedDirectories.GetPath(ProgramDirectory.Settings)) == false)
                {
                    MessageBox.Show("Не найдена директория для хранения пресетов ( " +
                        UsedDirectories.GetPath(ProgramDirectory.Settings) + " ).\n\n" +
                        "Данная директория будет создана рядом с исполняемым файлом.\n\n" +
                        "Нажмите ОК для продолжения.",
                        "Ошибка", MessageBoxButton.OK,
                        MessageBoxImage.Error, MessageBoxResult.OK);

                    Directory.CreateDirectory(UsedDirectories.GetPath(ProgramDirectory.Settings));
                }
            }
            catch (Exception error)
            {
                MessageBox.Show("Не удалось создать папку для хранения пресетов.\n\n" + error.Message,
                    "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.OK);
            }

            try
            {
                SystemOfPresets.FindFilesOfPresets(ref PresetFiles);

                for (int i = 0; i < PresetFiles.Length; i++)
                {
                    ComboBox_SelectedPreset.Items.Add(PresetFiles[i]);
                }
                
                if (SettingsDocument == String.Empty)
                {
                    Error_SettingsDocument = true;

                    MessageBox.Show("Файл настроек не определен.\n" +
                        "Нажмите ОК и выберите один из доступных пресетов в появившемся окне.",
                        "Ошибка", MessageBoxButton.OK,
                        MessageBoxImage.Error, MessageBoxResult.OK);

                    Select window = new Select(ref PresetFiles, "Выберите пресет");
                    window.ShowDialog();

                    if (window.SelectedDocumentPath != String.Empty)
                    {
                        SettingsDocument = window.SelectedDocumentPath;
                        Error_SettingsDocument = false;
                    }

                    else
                    {
                        MessageBox.Show("Пресет не выбран, программа будет закрыта.",
                            "Предупреждение", MessageBoxButton.OK,
                            MessageBoxImage.Warning, MessageBoxResult.OK);
                    }
                }

                else
                {
                    bool FileIsExisting = false;

                    foreach (string Path in PresetFiles)
                    {
                        if (Path == SettingsDocument)
                        {
                            FileIsExisting = true;
                            break;
                        }
                    }

                    if (FileIsExisting == false)
                    {
                        Error_SettingsDocument = true;

                        MessageBox.Show("Файл настроек \"" + SettingsDocument +
                            "\" не найден в папке " + UsedDirectories.GetPath(ProgramDirectory.Settings) + ".\n" +
                            "Нажмите ОК и выберите один из доступных пресетов в появившемся окне.",
                            "Ошибка", MessageBoxButton.OK,
                            MessageBoxImage.Error, MessageBoxResult.OK);

                        Select window = new Select(ref PresetFiles, "Выберите пресет");
                        window.ShowDialog();

                        if (window.SelectedDocumentPath != String.Empty)
                        {
                            SettingsDocument = window.SelectedDocumentPath;
                            Error_SettingsDocument = false;
                        }

                        else
                        {
                            MessageBox.Show("Пресет не выбран, программа будет закрыта.",
                                "Предупреждение", MessageBoxButton.OK,
                                MessageBoxImage.Warning, MessageBoxResult.OK);
                        }
                    }
                }

                if (Error_SettingsDocument)
                {
                    Application.Current.Shutdown();
                    return;
                }
            }
            catch(Exception error)
            {
                MessageBox.Show("Ошибка инициализации, программа будет закрыта.\n\n" + error.Message,
                   "Ошибка", MessageBoxButton.OK,
                   MessageBoxImage.Error, MessageBoxResult.OK);

                Application.Current.Shutdown();
                return;
            }

            UpdateDeviceData(SettingsDocument);
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

        private void SetUI_Connected()
        {
            Button_Connect.IsEnabled = false;
            Button_Disconnect.IsEnabled = true;
        }

        private void SetUI_Disconnected()
        {
            Button_Connect.IsEnabled = true;
            Button_Disconnect.IsEnabled = false;
        }

        private void MenuPreset_Save_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void MenuPreset_LoadMenuHelp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuHelp_Click(object sender, RoutedEventArgs e)
        {

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
                        Device.Connect(Settings.COMPort,
                                Convert.ToInt32(Settings.BaudRate),
                                Settings.Parity,
                                Convert.ToInt32(Settings.DataBits),
                                Settings.StopBits);
                        break;

                    case "Ethernet":
                        Device.Connect(Settings.IP, 
                            Convert.ToInt32(Settings.Port));
                        break;

                    default:
                        throw new Exception("В файле настроек задан неизвестный интерфейс связи.");
                }

                Device.AsyncDataReceived += Device_AsyncDataReceived;

                SetUI_Connected();
            }
            
            catch(Exception error)
            {
                MessageBox.Show("Возникла ошибка при подключении к устройству:\n\n" + error.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK,
                    MessageBoxOptions.ServiceNotification);
            }
        }

        private void Device_AsyncDataReceived(object sender, DataFromDevice e)
        {
            try
            {
                TextBlock_RX.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(delegate
                    {
                        TextBlock_RX.Text += e.RX;
                    }));
            }

            catch (Exception error)
            {
                MessageBox.Show("Возникла ошибка при приеме данных от устройства:\n" + error.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK,
                    MessageBoxOptions.ServiceNotification);
            }
        }


        private void Button_Disconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Device.Disconnect();

                SetUI_Disconnected();
            }

            catch(Exception error)
            {
                MessageBox.Show("Возникла ошибка при попытке отключения от устройства:\n" + error.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK,
                    MessageBoxOptions.ServiceNotification);
            }
        }

        private void TextBox_TX_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBox_TX.Text != String.Empty)
            {
                BytesToSend = TextBox_TX.Text.ToCharArray();
            }
        }

        private void Button_Send_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageType == TypeOfMessage.Char)
                {
                    return;
                }

                if (BytesToSend == null)
                {
                    MessageBox.Show("Буфер для отправления пуст. Введите в поле TX отправляемое значение.", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK,
                        MessageBoxOptions.ServiceNotification);

                    return;
                }

                Device.Send(new string(BytesToSend));
                TextBox_TX.Text = String.Empty;
                BytesToSend = null;
            }
            
            catch(Exception error)
            {
                MessageBox.Show("Возникла ошибка при отправлении данных устройству:\n" + error.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK,
                    MessageBoxOptions.ServiceNotification);
            }
        }

        private void SourceWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Enter:
                    Button_Send_Click(Button_Send, new RoutedEventArgs());
                    break;
            }

        }

        private void MenuSettings_Click(object sender, RoutedEventArgs e)
        {
            if (PresetFiles == null || PresetFiles.Length == 0)
            {
                MessageBox.Show("Не найдено ни одно файла настроек.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }

            SettingsWindow Window = new SettingsWindow(UsedDirectories.GetPath(ProgramDirectory.Settings), ref PresetFiles)
            {
                Owner = this
            };

            Window.ShowDialog();
        }

        private void RadioButton_Char_Checked(object sender, RoutedEventArgs e)
        {
            Button_Send.IsEnabled = false;
            MessageType = TypeOfMessage.Char;
        }

        private void RadioButton_String_Checked(object sender, RoutedEventArgs e)
        {
            Button_Send.IsEnabled = true;
            MessageType = TypeOfMessage.String;
        }

        
    }
}
