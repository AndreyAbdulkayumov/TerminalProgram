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
using System.IO.Ports;
using TerminalProgram.Device;

namespace TerminalProgram
{
    public struct ConnectionData
    {
        public string COMPort;
        public string BaudRate;
        public string Parity;
        public string DataBits;
        public string StopBits;
    }

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConnectedDevice Device = new ConnectedDevice();
        private ConnectionData DeviceSettings = new ConnectionData();

        private readonly string[] ArrayBaudRate = { "4800", "9600", "19200", "38400", "57600", "115200" };
        private readonly string[] ArrayParity = { "None", "Even", "Odd" };
        private readonly string[] ArrayDataBits = { "8", "9" };
        private readonly string[] ArrayStopBits = { "0", "1", "1.5", "2" };

        private const string StatusMessage_Connected = "Устройство подключено";
        private const string StatusMessage_Disconnected = "Нет подключенных устройств";

        private char[] BytesToSend;

        public MainWindow()
        {
            InitializeComponent();

            RadioButton_SerialPort.IsChecked = true;

            ComboBox_COMPort.AddHandler(ComboBox.MouseLeftButtonUpEvent, 
                new MouseButtonEventHandler(ComboBox_MouseLeftButtonDown), true);

            ComboBoxFilling(ComboBox_BaudRate, ref ArrayBaudRate);
            ComboBoxFilling(ComboBox_Parity, ref ArrayParity);
            ComboBoxFilling(ComboBox_DataBits, ref ArrayDataBits);
            ComboBoxFilling(ComboBox_StopBits, ref ArrayStopBits);

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

        private void SourceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SearchSerialPorts(ComboBox_COMPort);
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

        }

        private void TextBox_IP_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_Port_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Device.Connect(DeviceSettings.COMPort,
                                Convert.ToInt32(DeviceSettings.BaudRate),
                                DeviceSettings.Parity,
                                Convert.ToInt32(DeviceSettings.DataBits),
                                DeviceSettings.StopBits);

                Device.SerialPortReceived += Device_SerialPortReceived;

                TextBlock_DeviceStatus.Text = StatusMessage_Connected;
            }
            
            catch(Exception error)
            {
                MessageBox.Show("Возникла ошибка при подключении к устройству:\n" + error.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK,
                    MessageBoxOptions.ServiceNotification);
            }
        }

        private void Device_SerialPortReceived(object sender, DataFromDevice e)
        {
            try
            {
                TextBlock_RX.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(delegate () 
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

                TextBlock_DeviceStatus.Text = StatusMessage_Disconnected;
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
                if (BytesToSend == null)
                {
                    MessageBox.Show("Буфер для отправления пуст. Введите в поле TX отправляемое значение.", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK,
                        MessageBoxOptions.ServiceNotification);

                    return;
                }

                Device.Send(ref BytesToSend);
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

        private void ComboBox_COMPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeviceSettings.COMPort = ComboBox_COMPort.SelectedItem?.ToString();
        }

        private void ComboBox_BaudRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeviceSettings.BaudRate = ComboBox_BaudRate.SelectedItem?.ToString();
        }

        private void ComboBox_Parity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeviceSettings.Parity = ComboBox_Parity.SelectedItem?.ToString();
        }

        private void ComboBox_DataBits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeviceSettings.DataBits = ComboBox_DataBits.SelectedItem?.ToString();
        }

        private void ComboBox_StopBits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeviceSettings.StopBits = ComboBox_StopBits.SelectedItem?.ToString();
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
    }
}
