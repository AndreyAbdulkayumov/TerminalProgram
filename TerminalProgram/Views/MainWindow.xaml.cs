﻿using System;
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
using TerminalProgram.ViewModels;
using TerminalProgram.ViewModels.MainWindow;
using TerminalProgram.Views.Protocols;
using TerminalProgram.Views.Settings;
using System.Reactive.Linq;
using TerminalProgram.Views.ServiceWindows;

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

        public MainWindow()
        {
            InitializeComponent();

            MessageView.Title = this.Title; // Общий заголовок для всех диалоговых окон

            ViewModel = new ViewModel_CommonUI(
                MessageView.Show,
                SetUI_Connected,
                SetUI_Disconnected,
                Select_AvailablePresetFiles);

            DataContext = ViewModel;

            NoProtocolPage = new NoProtocol();

            ModbusPage = new Modbus();

            HttpPage = new Http();
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

        private string Select_AvailablePresetFiles(string[] Files)
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
                return "";
            }
        }

        private async void SourceWindow_Loaded(object sender, RoutedEventArgs e)
        {
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
            SettingsWindow Window = new SettingsWindow()
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
            AboutWindow window = new AboutWindow()
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

            GridRow_Header.Height = new GridLength(100);
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

            GridRow_Header.Height = new GridLength(100);
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

            GridRow_Header.Height = new GridLength(50);
            TextBlock_SelectedPreset.Visibility = Visibility.Hidden;
            ComboBox_SelectedPreset.Visibility = Visibility.Hidden;
            Button_Connect.Visibility = Visibility.Hidden;
            Button_Disconnect.Visibility = Visibility.Hidden;
        }
    }
}