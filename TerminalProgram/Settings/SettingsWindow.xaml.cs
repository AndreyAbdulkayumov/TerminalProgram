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
using System.Windows.Shapes;
using System.IO;
using TerminalProgram.Properties;
using System.Windows.Markup.Localizer;
using System.IO.Ports;

namespace TerminalProgram.Settings
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public bool SettingsIsChanged { get; private set; } = false;
        public string SettingsDocument { get; private set; }

        private DeviceData Settings = new DeviceData();

        private Page_IP Settings_IP = null;
        private Page_SerialPort Settings_SerialPort = null;

        private readonly string SettingsDocumentPath;

        private readonly string[] ArrayTypeOfEncoding = { "ASCII", "UTF-8", "Unicode", "UTF-7", "UTF-32" };


        public SettingsWindow(string SettingsPath, string SettingsFileName)
        {
            InitializeComponent();

            SettingsDocumentPath = SettingsPath;

            string[] Devices = MainWindow.GetDeviceList();
            ComboBoxFilling(ComboBox_SelectedDevice, ref Devices);

            ComboBoxFilling(ComboBox_SelectedEncoding, ref ArrayTypeOfEncoding);

            ComboBox_SelectedDevice.SelectionChanged -= ComboBox_SelectedDevice_SelectionChanged;

            foreach(string element in ComboBox_SelectedDevice.Items)
            {
                if (element == SettingsFileName)
                {
                    ComboBox_SelectedDevice.SelectedValue = element;
                    break;
                }
            }

            ComboBox_SelectedDevice.SelectionChanged += ComboBox_SelectedDevice_SelectionChanged;
        }

        private void ComboBoxFilling(ComboBox Box, ref string[] Items)
        {
            Box.Items.Clear();

            foreach(string Element in Items)
            {
                Box.Items.Add(Element);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Settings_SerialPort = new Page_SerialPort(ref Settings)
            {
                Height = Frame_Settings.ActualHeight,
                Width = Frame_Settings.ActualWidth,

                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            Settings_IP = new Page_IP(ref Settings)
            {
                Height = Frame_Settings.ActualHeight,
                Width = Frame_Settings.ActualWidth,

                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            if (await DisplaySettingsFile() == false)
            {
                Close();
            }
        }

        /// <summary>
        /// Метод отображения содержимого файла настроек на UI.
        /// </summary>
        /// <returns>
        /// true - если отображение прошло успешно, false - если пользователь проигнорировал все предупреждения.
        /// </returns>
        private async Task<bool> DisplaySettingsFile()
        {
            try
            {
                DeviceData Device = await SystemOfSettings.Read();

                if (Device == null)
                {
                    if (MessageBox.Show("В документе " + SystemOfSettings.Settings_FilePath +
                        " нет настроек устройства. Создать их?", "Предупреждение",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        Settings = SystemOfSettings.GetDefault();
                        RadioButton_SerialPort.IsChecked = true;
                    }

                    else
                    {
                        return false;
                    }
                }

                else
                {
                    Settings = Device;
                }

                UpdateCommonUI(Settings);

                Settings_SerialPort.UpdateUI(Settings);
                Settings_IP.UpdateUI(Settings);
            }

            catch (Exception error)
            {
                MessageBox.Show("Ошибка чтения данных из документа. " +
                    "Проверьте его целостность или выберите другой файл настроек.\n\n" +
                    error.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                return false;
            }

            return true;
        }

        private void UpdateCommonUI(DeviceData Data)
        {
            if (Data.GlobalEncoding == null)
            {
                ComboBox_SelectedEncoding.SelectedValue = null;
            }

            else
            {
                ComboBox_SelectedEncoding.SelectedValue =
                    ArrayTypeOfEncoding.Single(element => element == Data.GlobalEncoding);
            }

            SetValue(TextBox_Timeout_Write, Data.TimeoutWrite);

            SetValue(TextBox_Timeout_Read, Data.TimeoutRead);          

            switch (Data.TypeOfConnection)
            {
                case "SerialPort":
                    RadioButton_SerialPort.IsChecked = true;
                    break;

                case "Ethernet":
                    RadioButton_Ethernet.IsChecked = true;
                    break;

                default:
                    MessageBox.Show("В файле настроек задан неизвестный интерфейс связи: " + 
                        Data.TypeOfConnection.ToString() +
                        ".\n\nПо умолчанию будет выставлен SerialPort", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);

                    Data.TypeOfConnection = "SerialPort";
                    RadioButton_SerialPort.IsChecked = true;
                    break;
            }
        }

        private void SetValue(TextBox Box, string Value)
        {
            if (Value == null)
            {
                Box.Text = String.Empty;
                return;
            }

            Box.Text = Value;
        }
        
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    Button_File_Save_Click(Button_File_Save, new RoutedEventArgs());
                    break;

                case Key.Escape:
                    Close();
                    break;
            }
        }

        private async void ComboBox_SelectedDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string OldDocumentPath = SystemOfSettings.Settings_FilePath;

            try
            {
                if (ComboBox_SelectedDevice.SelectedItem == null)
                {
                    return;
                }

                await DisplaySettingsFile();

                SettingsDocument = ComboBox_SelectedDevice.SelectedItem.ToString();
            }

            catch(Exception error)
            {
                MessageBox.Show("Не удалось загрузить файл настроек: " + 
                    ComboBox_SelectedDevice.SelectedItem + 
                    Settings_SerialPort.Name + "\n\n" + error.Message, this.Title,
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                ComboBox_SelectedDevice.SelectedValue = 
                    System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetFileName(OldDocumentPath));
            }            
        }

        private void TextBox_Timeout_Write_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckNumber(TextBox_Timeout_Write);
            Settings.TimeoutWrite = TextBox_Timeout_Write.Text;
        }

        private void CheckNumber(TextBox Box)
        {
            string Text = Box.Text;

            if (Text == "")
            {
                return;
            }

            if (UInt32.TryParse(Text, out _) == false)
            {
                Box.Text = Text.Substring(0, Text.Length - 1);
                Box.SelectionStart = Box.Text.Length;

                MessageBox.Show("Разрешается вводить только неотрицательные целые числа.", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void TextBox_Timeout_Read_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckNumber(TextBox_Timeout_Read);
            Settings.TimeoutRead = TextBox_Timeout_Read.Text;
        }

        private void ComboBox_SelectedEncoding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_SelectedEncoding.SelectedItem != null)
            {
                Settings.GlobalEncoding = ComboBox_SelectedEncoding.SelectedItem.ToString();
            }   
        }

        private void RadioButton_SerialPort_Checked(object sender, RoutedEventArgs e)
        {
            if (Frame_Settings.Navigate(Settings_SerialPort) == false)
            {
                MessageBox.Show("Не удалось перейти на страницу " + Settings_SerialPort.Name, this.Title,
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }

            Settings.TypeOfConnection = "SerialPort";
        }

        private void RadioButton_Ethernet_Checked(object sender, RoutedEventArgs e)
        {
            if (Frame_Settings.Navigate(Settings_IP) == false)
            {
                MessageBox.Show("Не удалось перейти на страницу " + Settings_IP.Name, this.Title,
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }

            Settings.TypeOfConnection = "Ethernet";
        }
    }
}
