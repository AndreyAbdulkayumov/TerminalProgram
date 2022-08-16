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

namespace TerminalProgram.Protocols.NoProtocol
{
    public enum TypeOfMessage
    {
        Char,
        String
    };

    /// <summary>
    /// Логика взаимодействия для NoProtocol.xaml
    /// </summary>
    public partial class UI_NoProtocol : Page
    {
        private Connection Device = null;

        private TypeOfMessage MessageType;

        public UI_NoProtocol(MainWindow window, Connection ConnectedDevice)
        {
            InitializeComponent();

            window.DeviceIsConnect += MainWindow_DeviceIsConnect;
            window.DeviceIsDisconnected += MainWindow_DeviceIsDisconnected;

            Device = ConnectedDevice;

            Device.AsyncDataReceived += Device_AsyncDataReceived;
        }

        private void MainWindow_DeviceIsConnect(object sender, ConnectArgs e)
        {
            if (e.ConnectedDevice.IsConnected)
            {
                SetUI_Connected();
            }
        }

        private void MainWindow_DeviceIsDisconnected(object sender, ConnectArgs e)
        {
            TextBox_TX.Text = String.Empty;
            TextBlock_RX.Text = String.Empty;

            SetUI_Disconnected();
        }

        private void SetUI_Connected()
        {
            TextBox_TX.IsEnabled = true;

            CheckBox_CRCF.IsEnabled = true;
            RadioButton_Char.IsEnabled = true;
            RadioButton_String.IsEnabled = true;

            if (RadioButton_String.IsChecked == true)
            {
                Button_Send.IsEnabled = true;
            }

            else
            {
                Button_Send.IsEnabled = false;
            }

            TextBox_TX.Focus();
        }

        private void SetUI_Disconnected()
        {
            TextBox_TX.IsEnabled = false;

            CheckBox_CRCF.IsEnabled = false;
            RadioButton_Char.IsEnabled = false;
            RadioButton_String.IsEnabled = false;
            Button_Send.IsEnabled = false;
        }

        private void TextBox_TX_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TextBox_TX.Text != String.Empty && MessageType == TypeOfMessage.Char)
                {
                    Device.Send(TextBox_TX.Text.Substring(TextBox_TX.Text.Length - 1));
                }
            }

            catch (Exception error)
            {
                MessageBox.Show("Возникла ошибка при отправлении данных устройству:\n" + error.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        private void CheckBox_CRCF_Click(object sender, RoutedEventArgs e)
        {
            TextBox_TX.Focus();
        }

        private void RadioButton_Char_Checked(object sender, RoutedEventArgs e)
        {
            Button_Send.IsEnabled = false;

            MessageType = TypeOfMessage.Char;
            TextBox_TX.Text = String.Empty;

            TextBox_TX.Focus();
        }

        private void RadioButton_String_Checked(object sender, RoutedEventArgs e)
        {
            Button_Send.IsEnabled = true;

            MessageType = TypeOfMessage.String;
            TextBox_TX.Text = String.Empty;

            TextBox_TX.Focus();
        }

        private void Device_AsyncDataReceived(object sender, DataFromDevice e)
        {
            try
            {
                TextBlock_RX.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(delegate
                    {
                        TextBlock_RX.Text += e.RX;
                        ScrollViewer_RX.ScrollToEnd();
                    }));
            }

            catch (Exception error)
            {
                MessageBox.Show("Возникла ошибка при приеме данных от устройства:\n" + error.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK,
                    MessageBoxOptions.ServiceNotification);
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

                if (TextBox_TX.Text == String.Empty)
                {
                    MessageBox.Show("Буфер для отправления пуст. Введите в поле TX отправляемое значение.", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);

                    return;
                }

                if (CheckBox_CRCF.IsChecked == true)
                {
                    Device.Send(TextBox_TX.Text + "\r\n");
                }

                else
                {
                    Device.Send(TextBox_TX.Text);
                }
            }

            catch (Exception error)
            {
                MessageBox.Show("Возникла ошибка при отправлении данных устройству:\n" + error.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        private void Button_ClearFieldRX_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_RX.Text = String.Empty;

            TextBox_TX.Focus();
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    Button_Send_Click(Button_Send, new RoutedEventArgs());
                    break;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RadioButton_Char.IsChecked = true;
        }
    }
}
