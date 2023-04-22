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
using View_WPF.Protocols;

namespace View_WPF
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

            NoProtocolPage = new NoProtocol(this);

            ModbusPage = new Modbus(this);

            HttpPage = new Http(this);
        }

        private void SourceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RadioButton_NoProtocol.IsChecked = true;
        }

        private void SourceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void MenuSettings_Click(object sender, RoutedEventArgs e)
        {

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

            //SelectedProtocol = new ProtocolMode_NoProtocol(Client);
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

            //SelectedProtocol = new ProtocolMode_Modbus(Client, Settings);
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

        private void Button_Connect_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Disconnect_Click(object sender, RoutedEventArgs e)
        {

        }        
    }
}
