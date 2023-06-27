﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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
using View_WPF.ViewModels;
using View_WPF.ViewModels.MainWindow;

namespace View_WPF.Views.Protocols
{
    public enum TypeOfMessage
    {
        Char,
        String
    };

    /// <summary>
    /// Логика взаимодействия для NoProtocol.xaml
    /// </summary>
    public partial class NoProtocol : Page
    {
        private TypeOfMessage SendMessageType;

        private readonly string MainWindowTitle;

        private readonly ViewModel_NoProtocol ViewModel;

        public NoProtocol(MainWindow window)
        {
            InitializeComponent();

            MainWindowTitle = window.Title;

            ViewModel = new ViewModel_NoProtocol(
                MessageView.Show,
                SetUI_Connected, 
                SetUI_Disconnected,
                Action_Receive,
                Action_Clear_ReceiveField
                );

            DataContext = ViewModel;
        }

        private void SetUI_Connected()
        {
            TextBox_TX.IsEnabled = true;

            CheckBox_CR.IsEnabled = true;
            CheckBox_LF.IsEnabled = true;
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

            CheckBox_CR.IsEnabled = false;
            CheckBox_LF.IsEnabled = false;
            RadioButton_Char.IsEnabled = false;
            RadioButton_String.IsEnabled = false;
            Button_Send.IsEnabled = false;
        }

        private void Action_Receive(string Data)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send,
                    new Action(delegate
                    {
                        TextBox_RX.AppendText(Data);
                        TextBox_RX.LineDown();
                        ScrollViewer_RX.ScrollToEnd();
                    }));
        }

        private void Action_Clear_ReceiveField()
        {
            TextBox_RX.Clear();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RadioButton_String.Checked -= RadioButton_String_Checked;
            RadioButton_String.IsChecked = true;
            RadioButton_String.Checked += RadioButton_String_Checked;

            SendMessageType = TypeOfMessage.String;
            TextBox_TX.Text = String.Empty;

            TextBox_TX.Focus();
        }

        private async void Page_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        await ViewModel.Command_Send.Execute();
                        break;
                }
            }

            catch (Exception)
            {
                // Ошибка обрабатывавется во ViewModel
            }
        }

        private void CheckBox_CR_Click(object sender, RoutedEventArgs e)
        {
            TextBox_TX.Focus();
        }

        private void CheckBox_LF_Click(object sender, RoutedEventArgs e)
        {
            TextBox_TX.Focus();
        }

        private void RadioButton_Char_Checked(object sender, RoutedEventArgs e)
        {
            TextBox_TX.Focus();
        }

        private void RadioButton_String_Checked(object sender, RoutedEventArgs e)
        {
            TextBox_TX.Focus();
        }

        private void Button_SaveAs_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
