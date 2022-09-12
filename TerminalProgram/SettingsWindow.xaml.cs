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

namespace TerminalProgram
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public bool SettingsIsChanged { get; private set; } = false;
        public string SettingsDocument { get; private set; }

        private readonly string[] ArrayBaudRate = { "4800", "9600", "19200", "38400", "57600", "115200" };
        private readonly string[] ArrayParity = { "None", "Even", "Odd" };
        private readonly string[] ArrayDataBits = { "8", "9" };
        private readonly string[] ArrayStopBits = { "0", "1", "1.5", "2" };

        private readonly SettingsMediator SettingsManager = new SettingsMediator();
        private DeviceData Settings = new DeviceData();

        private string SettingsDocumentPath;

        public SettingsWindow(string Directory, ref string[] PresetFiles)
        {
            InitializeComponent();

            ComboBoxFilling(ComboBox_SelectedDevice, ref PresetFiles);

            ComboBox_SelectedDevice.SelectedIndex = 0;

            SettingsDocumentPath = Directory + PresetFiles[0] + ".xml";
            SettingsDocument = PresetFiles[0];

            ComboBoxFilling(ComboBox_BaudRate, ref ArrayBaudRate);
            ComboBoxFilling(ComboBox_Parity, ref ArrayParity);
            ComboBoxFilling(ComboBox_DataBits, ref ArrayDataBits);
            ComboBoxFilling(ComboBox_StopBits, ref ArrayStopBits);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ComboBox_COMPort.AddHandler(ComboBox.MouseLeftButtonUpEvent,
                    new MouseButtonEventHandler(ComboBox_MouseLeftButtonDown), true);

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

            SetValue(ComboBox_COMPort, Data.COMPort);
            SetValue(ComboBox_BaudRate, Data.BaudRate);
            SetValue(ComboBox_Parity, Data.Parity);
            SetValue(ComboBox_DataBits, Data.DataBits);
            SetValue(ComboBox_StopBits, Data.StopBits);

            SetValue(TextBox_IP, Data.IP);
            SetValue(TextBox_Port, Data.Port);

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

        private void SetValue(ComboBox Box, string Value)
        {
            if (Value == SettingsManager.DefaultNodeValue)
            {
                Box.SelectedIndex = -1;
                return;
            }

            int index = Box.Items.IndexOf(Value);

            if (index < 0)
            {
                return;
            }

            Box.SelectedIndex = index;
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

        private void ComboBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SearchSerialPorts(ComboBox_COMPort);
        }

        private void SearchSerialPorts(ComboBox Box)
        {
            string[] ports = SerialPort.GetPortNames();

            if (ports.Length != Box.Items.Count)
            {
                int CountItems = Box.Items.Count;

                for (int i = 0; i < CountItems; i++)
                {
                    Box.Items.RemoveAt(0);
                }

                foreach (string port in ports)
                {
                    Box.Items.Add(port);
                }
            }
        }

        private void ComboBoxFilling(ComboBox Box, ref string[] Items)
        {
            for (int i = 0; i < Items.Length; i++)
            {
                Box.Items.Add(Items[i]);
            }
        }
        private void RadioButton_SerialPort_Checked(object sender, RoutedEventArgs e)
        {
            TextBlock_COMPort.Visibility = Visibility.Visible;
            ComboBox_COMPort.Visibility = Visibility.Visible;
            TextBlock_BaudRate.Visibility = Visibility.Visible;
            ComboBox_BaudRate.Visibility = Visibility.Visible;
            TextBlock_Parity.Visibility = Visibility.Visible;
            ComboBox_Parity.Visibility = Visibility.Visible;
            TextBlock_DataBits.Visibility = Visibility.Visible;
            ComboBox_DataBits.Visibility = Visibility.Visible;
            TextBlock_StopBits.Visibility = Visibility.Visible;
            ComboBox_StopBits.Visibility = Visibility.Visible;

            TextBlock_IP.Visibility = Visibility.Hidden;
            TextBox_IP.Visibility = Visibility.Hidden;
            TextBlock_Port.Visibility = Visibility.Hidden;
            TextBox_Port.Visibility = Visibility.Hidden;

            Settings.TypeOfConnection = "SerialPort";
        }

        private void RadioButton_Ethernet_Checked(object sender, RoutedEventArgs e)
        {
            TextBlock_COMPort.Visibility = Visibility.Hidden;
            ComboBox_COMPort.Visibility = Visibility.Hidden;
            TextBlock_BaudRate.Visibility = Visibility.Hidden;
            ComboBox_BaudRate.Visibility = Visibility.Hidden;
            TextBlock_Parity.Visibility = Visibility.Hidden;
            ComboBox_Parity.Visibility = Visibility.Hidden;
            TextBlock_DataBits.Visibility = Visibility.Hidden;
            ComboBox_DataBits.Visibility = Visibility.Hidden;
            TextBlock_StopBits.Visibility = Visibility.Hidden;
            ComboBox_StopBits.Visibility = Visibility.Hidden;

            TextBlock_IP.Visibility = Visibility.Visible;
            TextBox_IP.Visibility = Visibility.Visible;
            TextBlock_Port.Visibility = Visibility.Visible;
            TextBox_Port.Visibility = Visibility.Visible;

            Settings.TypeOfConnection = "Ethernet";
        }

        private void TextBox_IP_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.IP = TextBox_IP.Text;
        }

        private void TextBox_Port_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Port = TextBox_Port.Text;
        }

        private void ComboBox_COMPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.COMPort = ComboBox_COMPort.SelectedItem?.ToString();
        }

        private void ComboBox_BaudRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.BaudRate = ComboBox_BaudRate.SelectedItem?.ToString();
        }

        private void ComboBox_Parity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Parity = ComboBox_Parity.SelectedItem?.ToString();
        }

        private void ComboBox_DataBits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.DataBits = ComboBox_DataBits.SelectedItem?.ToString();
        }

        private void ComboBox_StopBits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.StopBits = ComboBox_StopBits.SelectedItem?.ToString();
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
    }
}
