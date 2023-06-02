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
using View_WPF.ViewModels;
using View_WPF.ViewModels.MainWindow;
using View_WPF.Views.Protocols;
using View_WPF.Views.Settings;

namespace View_WPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NoProtocol NoProtocolPage;
        private Modbus ModbusPage;
        private Http HttpPage;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new ViewModel_CommonUI(
                MessageBoxView,
                SetUI_Connected,
                SetUI_Disconnected);

            NoProtocolPage = new NoProtocol(this);

            ModbusPage = new Modbus(this);

            HttpPage = new Http(this);

            ComboBox_SelectedPreset.Items.Add("Item_1.qw");
            ComboBox_SelectedPreset.Items.Add("Item_2.qw");
            ComboBox_SelectedPreset.Items.Add("Item_3.qw");
            ComboBox_SelectedPreset.Items.Add("Item_4.qw");
            ComboBox_SelectedPreset.SelectedIndex = 0;
        }

        private void MessageBoxView(string Message, MessageType Type)
        {
            MessageBoxImage Image;

            switch (Type)
            {
                case MessageType.Error:
                    Image = MessageBoxImage.Error;
                    break;

                case MessageType.Warning:
                    Image = MessageBoxImage.Warning;
                    break;

                case MessageType.Information:
                    Image = MessageBoxImage.Information;
                    break;

                default:
                    Image = MessageBoxImage.Information;
                    break;
            }

            MessageBox.Show(Message, this.Title, MessageBoxButton.OK, Image);
        }

        private void SetUI_Connected()
        {
            MenuSettings.IsEnabled = false;

            ComboBox_SelectedPreset.IsEnabled = false;

            Button_Connect.IsEnabled = false;
            Button_Disconnect.IsEnabled = true;            
        }

        private void SetUI_Disconnected()
        {
            MenuSettings.IsEnabled = true;

            ComboBox_SelectedPreset.IsEnabled = true;

            Button_Connect.IsEnabled = true;
            Button_Disconnect.IsEnabled = false;            
        }

        private void SourceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RadioButton_NoProtocol.IsChecked = true;
        }

        private void SourceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Button_MinimizeApplication_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Button_CloseApplication_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow Window = new SettingsWindow()
            {
                Owner = this
            };

            Window.ShowDialog();
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow window = new AboutWindow()
            {
                Owner = this
            };

            window.ShowDialog();
        }

        private void RadioButton_NoProtocol_Checked(object sender, RoutedEventArgs e)
        {
            if (NoProtocolPage == null)
            {
                return;
            }

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
        }

        private void RadioButton_Protocol_Modbus_Checked(object sender, RoutedEventArgs e)
        {
            if (ModbusPage == null)
            {
                return;
            }

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
        }

        private void RadioButton_Protocol_Http_Checked(object sender, RoutedEventArgs e)
        {
            if (HttpPage == null)
            {
                return;
            }

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

        private void ComboBox_SelectedPreset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
