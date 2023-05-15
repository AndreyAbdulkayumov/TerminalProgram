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
using View_WPF.ViewModels;

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

        public NoProtocol(MainWindow window)
        {
            InitializeComponent();

            MainWindowTitle = window.Title;

            DataContext = new ViewModel_NoProtocol(MessageBoxView);
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

            MessageBox.Show(Message, MainWindowTitle, MessageBoxButton.OK, Image);
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

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    Button_Send_Click(Button_Send, new RoutedEventArgs());
                    break;
            }
        }

        private void TextBox_TX_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CheckBox_CR_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_LF_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButton_Char_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButton_String_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Send_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_SaveAs_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_ClearFieldRX_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
