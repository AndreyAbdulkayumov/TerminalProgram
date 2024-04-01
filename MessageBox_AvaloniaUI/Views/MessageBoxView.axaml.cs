using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MessageBox_Core;
using System;
using System.Collections.ObjectModel;

namespace MessageBox_AvaloniaUI.Views
{
    public enum MessageBoxToolType
    {
        Default,
        YesNo
    }    

    public class ButtonContent
    {
        public string Content { get; set; }

        public ButtonContent(string Content)
        {
            this.Content = Content;
        }
    }

    public partial class MessageBoxView : Window
    {
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.Default;

        public ObservableCollection<ButtonContent> Buttons { get; set; } = new ObservableCollection<ButtonContent>();

        private const string Content_OK = "ОК";
        private const string Content_Yes = "Да";
        private const string Content_No = "Нет";

        public MessageBoxView(string Message, string Title, MessageBoxToolType Type)
        {
            InitializeComponent();

            DataContext = this;

            TextBlock_Content.Text = Message;
            TextBlock_Title.Text = Title;

            switch (Type)
            {
                case MessageBoxToolType.Default:
                    Buttons.Add(new ButtonContent(Content_OK));
                    break;

                case MessageBoxToolType.YesNo:
                    Buttons.Add(new ButtonContent(Content_Yes));
                    Buttons.Add(new ButtonContent(Content_No));
                    break;

                default:
                    Buttons.Add(new ButtonContent(Content_OK));
                    break;
            }
        }

        private void Button_Click(object? sender, RoutedEventArgs e)
        {
            Button? ClickedButton = sender as Button;

            if (ClickedButton != null)
            {
                switch (ClickedButton.Content)
                {
                    case Content_Yes:
                        Result = MessageBoxResult.Yes;
                        break;

                    case Content_No:
                        Result = MessageBoxResult.No;
                        break;
                }

                this.Close();
            }
        }

        private void Window_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape ||
                e.Key == Key.Enter)
            {
                this.Close();
            }
        }

        private void Chrome_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            this.BeginMoveDrag(e);
        }

        private void Button_Close_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
