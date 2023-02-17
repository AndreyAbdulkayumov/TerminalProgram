using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TerminalProgram.Settings
{
    /// <summary>
    /// Логика взаимодействия для Page_SerialPort.xaml
    /// </summary>
    public partial class Page_SerialPort : Page
    {
        private readonly string[] ArrayBaudRate = { "4800", "9600", "19200", "38400", "57600", "115200" };
        private readonly string[] ArrayParity = { "None", "Even", "Odd" };
        private readonly string[] ArrayDataBits = { "5", "6", "7", "8" };
        private readonly string[] ArrayStopBits = { "0", "1", "1.5", "2" };

        private DeviceData Settings;

        public Page_SerialPort(ref DeviceData Settings)
        {
            InitializeComponent();

            this.Settings = Settings;

            ComboBoxFilling(ComboBox_BaudRate, ref ArrayBaudRate);
            ComboBoxFilling(ComboBox_Parity, ref ArrayParity);
            ComboBoxFilling(ComboBox_DataBits, ref ArrayDataBits);
            ComboBoxFilling(ComboBox_StopBits, ref ArrayStopBits);
        }

        public void UpdateUI(DeviceData UpdateSettings)
        {
            Settings = UpdateSettings;

            Button_ReScan_COMPorts_Click(this, new RoutedEventArgs());

            SetValue(ComboBox_BaudRate, UpdateSettings.Connection_SerialPort.BaudRate);

            if (UpdateSettings.Connection_SerialPort.BaudRate_Custom == null)
            {
                UpdateSettings.Connection_SerialPort.BaudRate_Custom = String.Empty;
            }

            TextBox_BaudRate_Custom.Text = UpdateSettings.Connection_SerialPort.BaudRate_Custom;

            if (UpdateSettings.Connection_SerialPort.BaudRate_IsCustom == "Enable")
            {
                CheckBox_BaudRate_Custom_Enable.IsChecked = true;
            }

            else
            {
                CheckBox_BaudRate_Custom_Enable.IsChecked = false;
            }

            CheckBox_BaudRate_Custom_Enable_Click(CheckBox_BaudRate_Custom_Enable, new RoutedEventArgs());

            SetValue(ComboBox_Parity, UpdateSettings.Connection_SerialPort.Parity);
            SetValue(ComboBox_DataBits, UpdateSettings.Connection_SerialPort.DataBits);
            SetValue(ComboBox_StopBits, UpdateSettings.Connection_SerialPort.StopBits);
        }

        private void SetValue(ComboBox Box, string Value)
        {
            if (Value == null)
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

        private void ComboBoxFilling(ComboBox Box, ref string[] Items)
        {
            for (int i = 0; i < Items.Length; i++)
            {
                Box.Items.Add(Items[i]);
            }
        }

        private void ComboBox_COMPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_COMPort.SelectedIndex != -1)
            {
                Settings.Connection_SerialPort.COMPort = ComboBox_COMPort.SelectedItem?.ToString();
            }            
        }

        private void ComboBox_BaudRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_BaudRate.SelectedIndex != -1)
            {
                Settings.Connection_SerialPort.BaudRate = ComboBox_BaudRate.SelectedItem?.ToString();
            }
        }
        
        private void CheckBox_BaudRate_Custom_Enable_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBox_BaudRate_Custom_Enable.IsChecked == true)
            {
                ComboBox_BaudRate.SelectedIndex = -1;
                ComboBox_BaudRate.IsEnabled = false;

                TextBox_BaudRate_Custom.Text = Settings.Connection_SerialPort.BaudRate_Custom;
                TextBox_BaudRate_Custom.IsEnabled = true;

                Settings.Connection_SerialPort.BaudRate_IsCustom = "Enable";
            }

            else
            {
                SetValue(ComboBox_BaudRate, Settings.Connection_SerialPort.BaudRate);
                ComboBox_BaudRate.IsEnabled = true;

                TextBox_BaudRate_Custom.Text = String.Empty;
                TextBox_BaudRate_Custom.IsEnabled = false;

                Settings.Connection_SerialPort.BaudRate_IsCustom = "Disable";
            }
        }

        private void TextBox_BaudRate_Custom_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (TextBox_BaudRate_Custom.Text.Length == 0)
            {
                return;
            }

            if (UInt32.TryParse(TextBox_BaudRate_Custom.Text, out _) == false)
            {
                MessageBox.Show("В поле Custom BaudRate можно ввести только положительное целочисленное значение.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                TextBox_BaudRate_Custom.Text = TextBox_BaudRate_Custom.Text.Substring(0, TextBox_BaudRate_Custom.Text.Length - 1);
                TextBox_BaudRate_Custom.SelectionStart = TextBox_BaudRate_Custom.Text.Length;

                return;
            }

            Settings.Connection_SerialPort.BaudRate_Custom = TextBox_BaudRate_Custom.Text;
        }

        private void ComboBox_Parity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Connection_SerialPort.Parity = ComboBox_Parity.SelectedItem?.ToString();
        }

        private void ComboBox_DataBits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Connection_SerialPort.DataBits = ComboBox_DataBits.SelectedItem?.ToString();
        }

        private void ComboBox_StopBits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Connection_SerialPort.StopBits = ComboBox_StopBits.SelectedItem?.ToString();
        }

        private void Button_ReScan_COMPorts_Click(object sender, RoutedEventArgs e)
        {
            string[] PortsList = SerialPort.GetPortNames();

            ComboBox_COMPort.Items.Clear();

            foreach (string Port in PortsList)
            {
                ComboBox_COMPort.Items.Add(Port);
            }

            if (Settings.Connection_SerialPort.COMPort == null)
            {
                ComboBox_COMPort.SelectedIndex = -1;
                TextBlock_PortNotFound.Text = "Порт не задан";
                Border_PortNotFound.Visibility = Visibility.Visible;

                return;
            }

            string SelectedPort = Settings.Connection_SerialPort.COMPort;
            string FoundPort = null;

            foreach (string Port in ComboBox_COMPort.Items)
            {
                if (Port == SelectedPort)
                {
                    FoundPort = Port;
                    break;
                }
            }

            if (FoundPort == null)
            {
                ComboBox_COMPort.SelectedIndex = -1;
                TextBlock_PortNotFound.Text = "Порт " + SelectedPort + " не найден";
                Border_PortNotFound.Visibility = Visibility.Visible;
            }

            else
            {
                ComboBox_COMPort.SelectedItem = FoundPort;
                Border_PortNotFound.Visibility = Visibility.Hidden;
            }
        }
    }
}
