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
using System.Reactive.Linq;
using ViewModels.MainWindow;
using TerminalProgram.Views.Protocols;
using TerminalProgram.Views.Settings;
using TerminalProgram.Views.ServiceWindows;
using TerminalProgram.Themes;
using MessageBox_WPF;
using MessageBox_Core;

namespace TerminalProgram.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly NoProtocol NoProtocolPage;
        private readonly Modbus ModbusPage;
        private readonly Http HttpPage;

        private readonly ViewModel_CommonUI ViewModel;

        private readonly WPF_MessageView MessageView;

        private readonly GridLength RowLength_Connection;
        private readonly GridLength RowLength_Info;

        public MainWindow()
        {
            InitializeComponent();

            MessageView = new WPF_MessageView(this.Title); ; // Общий заголовок для всех диалоговых окон

            Properties.Settings.Default.Reload();

            // Темная тема по умолчанию
            if (Properties.Settings.Default.ThemeName == String.Empty)
            {
                Properties.Settings.Default.ThemeName = ThemesManager.ThemeTypeName_Dark;
                Properties.Settings.Default.Save();
            }

            ViewModel = new ViewModel_CommonUI(
                MessageView.Show,
                SetUI_Connected,
                SetUI_Disconnected,
                Select_AvailablePresetFiles,
                Properties.Settings.Default.SettingsDocument,
                Properties.Settings.Default.ThemeName,
                ThemesManager.ThemeTypeName_Dark,
                ThemesManager.ThemeTypeName_Light
                );

            ViewModel_CommonUI.SettingsDocument_Changed += ViewModel_CommonUI_SettingsDocument_Changed;
            ViewModel_CommonUI.ThemeName_Changed += ViewModel_CommonUI_ThemeName_Changed;

            DataContext = ViewModel;           

            NoProtocolPage = new NoProtocol(MessageView);

            ModbusPage = new Modbus(MessageView);

            HttpPage = new Http(MessageView);

            RowLength_Connection = GridRow_Connection.Height;
            RowLength_Info = GridRow_Info.Height;
        }

        private void ViewModel_CommonUI_SettingsDocument_Changed(object? sender, DocArgs e)
        {
            if (e.FilePath == null)
            {
                return;
            }

            Properties.Settings.Default.Reload();
            Properties.Settings.Default.SettingsDocument = e.FilePath;
            Properties.Settings.Default.Save();
        }

        private void ViewModel_CommonUI_ThemeName_Changed(object? sender, DocArgs e)
        {
            if (e.FilePath == null)
            {
                return;
            }

            Properties.Settings.Default.Reload();
            Properties.Settings.Default.ThemeName = e.FilePath;
            Properties.Settings.Default.Save();

            ThemesManager.Select(e.FilePath);
        }        

        private void SetUI_Connected()
        {
            MenuItem_Settings.IsEnabled = false;

            ComboBox_SelectedPreset.IsEnabled = false;

            Button_Connect.IsEnabled = false;
            Button_Disconnect.IsEnabled = true;            
        }

        private void SetUI_Disconnected()
        {
            MenuItem_Settings.IsEnabled = true;

            ComboBox_SelectedPreset.IsEnabled = true;

            Button_Connect.IsEnabled = true;
            Button_Disconnect.IsEnabled = false;            
        }

        private string? Select_AvailablePresetFiles(string[] Files)
        {
            ComboBoxWindow window = new ComboBoxWindow(Files)
            {
                Owner = this
            };

            window.ShowDialog();

            if (window.SelectedDocumentPath != String.Empty)
            {
                return window.SelectedDocumentPath;
            }

            else
            {
                Application.Current.Shutdown();
                return null;
            }
        }

        private async void SourceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reload();

            ThemesManager.Select(Properties.Settings.Default.ThemeName);

            RadioButton_NoProtocol.IsChecked = true;

            await ViewModel.Command_UpdatePresets.Execute();
        }

        private async void SourceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (ViewModel.IsConnected)
                {
                    if (MessageBox.Show("Клиент ещё подключен к хосту.\nЗакрыть программу?", MessageView.Title,
                        MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }

                    await ViewModel.Command_Disconnect.Execute();
                }
            }

            catch (Exception error)
            {
                MessageView.Show("Возникла ошибка во время закрытия программы.\n\n" + error.Message, MessageType.Error);
            }
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
            this.Close();
        }

        private async void MenuSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow Window = new SettingsWindow(MessageView)
            {
                Owner = this
            };

            Window.ShowDialog();

            if (ViewModel != null)
            {
                await ViewModel.Command_UpdatePresets.Execute();
            }
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow window = new AboutWindow(MessageView)
            {
                Owner = this
            };

            window.ShowDialog();
        }

        private void RadioButton_NoProtocol_Checked(object sender, RoutedEventArgs e)
        {
            if (Frame_ActionUI.Navigate(NoProtocolPage) == false)
            {
                MessageView.Show("Не удалось перейти на страницу " + NoProtocolPage.Name, MessageType.Error);
                return;
            }

            GridRow_Connection.Height = RowLength_Connection;
            GridRow_Info.Height = RowLength_Info;

            TextBlock_SelectedPreset.Visibility = Visibility.Visible;
            ComboBox_SelectedPreset.Visibility = Visibility.Visible;
            Button_Connect.Visibility = Visibility.Visible;
            Button_Disconnect.Visibility = Visibility.Visible;
        }

        private void RadioButton_Protocol_Modbus_Checked(object sender, RoutedEventArgs e)
        {
            if (Frame_ActionUI.Navigate(ModbusPage) == false)
            {
                MessageView.Show("Не удалось перейти на страницу " + ModbusPage.Name, MessageType.Error);
                return;
            }

            GridRow_Connection.Height = RowLength_Connection;
            GridRow_Info.Height = RowLength_Info;

            TextBlock_SelectedPreset.Visibility = Visibility.Visible;
            ComboBox_SelectedPreset.Visibility = Visibility.Visible;
            Button_Connect.Visibility = Visibility.Visible;
            Button_Disconnect.Visibility = Visibility.Visible;
        }

        private void RadioButton_Protocol_Http_Checked(object sender, RoutedEventArgs e)
        {
            if (Frame_ActionUI.Navigate(HttpPage) == false)
            {
                MessageView.Show("Не удалось перейти на страницу " + HttpPage.Name, MessageType.Error);
                return;
            }

            GridRow_Connection.Height = new GridLength(0);
            GridRow_Info.Height = new GridLength(0);

            TextBlock_SelectedPreset.Visibility = Visibility.Hidden;
            ComboBox_SelectedPreset.Visibility = Visibility.Hidden;
            Button_Connect.Visibility = Visibility.Hidden;
            Button_Disconnect.Visibility = Visibility.Hidden;
        }
    }
}
