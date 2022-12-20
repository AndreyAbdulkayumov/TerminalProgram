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
using SystemOfSaving;

namespace TerminalProgram.Settings
{
    /// <summary>
    /// Логика взаимодействия для Page_SerialPort.xaml
    /// </summary>
    public partial class Page_SerialPort : Page
    {
        private readonly string[] ArrayBaudRate = { "4800", "9600", "19200", "38400", "57600", "115200" };
        private readonly string[] ArrayParity = { "None", "Even", "Odd" };
        private readonly string[] ArrayDataBits = { "8", "9" };
        private readonly string[] ArrayStopBits = { "0", "1", "1.5", "2" };

        private DeviceData Settings;
        private readonly string DefaultValue;

        public Page_SerialPort(ref DeviceData Settings, string DefaultValue)
        {
            InitializeComponent();

            this.Settings = Settings;
            this.DefaultValue = DefaultValue;

            ComboBoxFilling(ComboBox_BaudRate, ref ArrayBaudRate);
            ComboBoxFilling(ComboBox_Parity, ref ArrayParity);
            ComboBoxFilling(ComboBox_DataBits, ref ArrayDataBits);
            ComboBoxFilling(ComboBox_StopBits, ref ArrayStopBits);
        }

        public void UpdateUI(DeviceData UpdateSettings)
        {
            ComboBox_COMPort.AddHandler(ComboBox.MouseLeftButtonUpEvent,
                    new MouseButtonEventHandler(ComboBox_MouseLeftButtonDown), true);

            SetValue(ComboBox_COMPort, UpdateSettings.COMPort);

            SetValue(ComboBox_BaudRate, UpdateSettings.BaudRate);

            if (UpdateSettings.BaudRate_Custom == DefaultValue)
            {
                UpdateSettings.BaudRate_Custom = String.Empty;
            }

            TextBox_BaudRate_Custom.Text = UpdateSettings.BaudRate_Custom;

            if (UpdateSettings.BaudRate_IsCustom == "Enable")
            {
                CheckBox_BaudRate_Custom_Enable.IsChecked = true;
            }

            else
            {
                CheckBox_BaudRate_Custom_Enable.IsChecked = false;
            }

            CheckBox_BaudRate_Custom_Enable_Click(CheckBox_BaudRate_Custom_Enable, new RoutedEventArgs());

            SetValue(ComboBox_Parity, UpdateSettings.Parity);
            SetValue(ComboBox_DataBits, UpdateSettings.DataBits);
            SetValue(ComboBox_StopBits, UpdateSettings.StopBits);

            Settings = UpdateSettings;
        }

        private void SetValue(ComboBox Box, string Value)
        {
            if (Value == DefaultValue)
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

        private void ComboBox_COMPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.COMPort = ComboBox_COMPort.SelectedItem?.ToString();
        }

        private void ComboBox_BaudRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_BaudRate.SelectedIndex != -1)
            {
                Settings.BaudRate = ComboBox_BaudRate.SelectedItem?.ToString();
            }
        }
        
        private void CheckBox_BaudRate_Custom_Enable_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBox_BaudRate_Custom_Enable.IsChecked == true)
            {
                ComboBox_BaudRate.SelectedIndex = -1;
                ComboBox_BaudRate.IsEnabled = false;

                TextBox_BaudRate_Custom.Text = Settings.BaudRate_Custom;
                TextBox_BaudRate_Custom.IsEnabled = true;

                Settings.BaudRate_IsCustom = "Enable";
            }

            else
            {
                SetValue(ComboBox_BaudRate, Settings.BaudRate);
                ComboBox_BaudRate.IsEnabled = true;

                TextBox_BaudRate_Custom.Text = String.Empty;
                TextBox_BaudRate_Custom.IsEnabled = false;

                Settings.BaudRate_IsCustom = "Disable";
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

            Settings.BaudRate_Custom = TextBox_BaudRate_Custom.Text;
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
    }
}
