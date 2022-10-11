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
using System.IO.Ports;
using SystemOfSaving;

namespace TerminalProgram.Settings
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public bool SettingsIsChanged { get; private set; } = false;
        public string SettingsDocument { get; private set; }

        private readonly SettingsMediator SettingsManager = new SettingsMediator();
        private DeviceData Settings = new DeviceData();

        private Page_IP Settings_IP = null;
        private Page_SerialPort Settings_SerialPort = null;

        private readonly string SettingsDocumentPath;

        private string[] ArrayTypeOfEncoding = { "ASCII", "UTF-8", "Unicode", "UTF-7", "UTF-32" };

        public SettingsWindow(string Directory, ref string[] PresetFiles)
        {
            InitializeComponent();
                        
            ComboBoxFilling(ComboBox_SelectedDevice, ref PresetFiles);
            ComboBox_SelectedDevice.SelectedIndex = 0;

            SettingsDocumentPath = Directory + PresetFiles[0] + ".xml";
            SettingsDocument = PresetFiles[0];            
        }

        private void ComboBoxFilling(ComboBox Box, ref string[] Items)
        {
            for (int i = 0; i < Items.Length; i++)
            {
                Box.Items.Add(Items[i]);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                SettingsManager.LoadSettingsFrom(SettingsDocumentPath);

                List<string> Devices = SettingsManager.GetAllDevicesNames();

                if (Devices.Count > 0)
                {
                    Settings = SettingsManager.GetDeviceData(Devices[0]);
                }

                else
                {
                    if (MessageBox.Show("В документе " + SettingsDocumentPath + " нет настроек устройства. Создать их?", "Предупреждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        SettingsManager.CreateDevice("Default Name");
                        RadioButton_SerialPort.IsChecked = true;
                        return;
                    }

                    else
                    {
                        Close();
                        return;
                    }
                }

                ComboBoxFilling(ComboBox_SelectedEncoding, ref ArrayTypeOfEncoding);
                ComboBox_SelectedEncoding.SelectedValue = ArrayTypeOfEncoding.Single(element => element == Settings.GlobalEncoding);

                Settings_SerialPort = new Page_SerialPort(Settings, SettingsManager)
                {
                    Height = Frame_Settings.ActualHeight,
                    Width = Frame_Settings.ActualWidth,

                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };

                Settings_IP = new Page_IP(Settings, SettingsManager)
                {
                    Height = Frame_Settings.ActualHeight,
                    Width = Frame_Settings.ActualWidth,

                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };
            }

            catch(Exception error)
            {
                MessageBox.Show("Ошибка чтения данных из документа. Проверьте его целостность или выберите другой файл настроек.\n\n" + error.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                Close();
                return;
            }

            UpdateUI(Settings);
        }


        private void UpdateUI(DeviceData Data)
        {
            SetValue(TextBox_Timeout_Write, Data.TimeoutWrite);

            if (Data.TimeoutWrite_IsInfinite == "Enable")
            {
                CheckBox_Timeout_Write_Infinite.IsChecked = true;
                TextBox_Timeout_Write.IsEnabled = false;
            }

            else
            {
                CheckBox_Timeout_Write_Infinite.IsChecked = false;
                TextBox_Timeout_Write.IsEnabled = true;
            }

            SetValue(TextBox_Timeout_Read, Data.TimeoutRead);

            if (Data.TimeoutRead_IsInfinite == "Enable")
            {
                CheckBox_Timeout_Read_Infinite.IsChecked = true;
                TextBox_Timeout_Read.IsEnabled = false;
            }

            else
            {
                CheckBox_Timeout_Read_Infinite.IsChecked = false;
                TextBox_Timeout_Read.IsEnabled = true;
            }

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

                    RadioButton_SerialPort.IsChecked = true;
                    break;
            }
        }

        private void SetValue(TextBox Box, string Value)
        {
            if (Value == SettingsManager.DefaultNodeValue)
            {
                Box.Text = String.Empty;
                return;
            }

            Box.Text = Value;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }
        
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    Button_Save_Click(Button_Save, new RoutedEventArgs());
                    break;

                case Key.Escape:
                    Close();
                    break;
            }
        }

        private void ComboBox_SelectedDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingsDocument = ComboBox_SelectedDevice.SelectedItem.ToString();
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

        private void CheckBox_Timeout_Write_Infinite_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBox_Timeout_Write_Infinite.IsChecked == true)
            {
                TextBox_Timeout_Write.IsEnabled = false;
                Settings.TimeoutWrite_IsInfinite = "Enable";
            }

            else
            {
                TextBox_Timeout_Write.IsEnabled = true;
                Settings.TimeoutWrite_IsInfinite = "Disable";
            }
        }

        private void TextBox_Timeout_Read_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckNumber(TextBox_Timeout_Read);
            Settings.TimeoutRead = TextBox_Timeout_Read.Text;
        }

        private void CheckBox_Timeout_Read_Infinite_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBox_Timeout_Read_Infinite.IsChecked == true)
            {
                TextBox_Timeout_Read.IsEnabled = false;
                Settings.TimeoutRead_IsInfinite = "Enable";
            }

            else
            {
                TextBox_Timeout_Read.IsEnabled = true;
                Settings.TimeoutRead_IsInfinite = "Disable";
            }
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

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.Save(Settings);

            SettingsIsChanged = true;

            MessageBox.Show("Настройки успешно сохранены!", "",
                    MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
