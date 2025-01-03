using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MessageBox_Core;
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

        public MessageBoxView(string message, string title, MessageBoxToolType type)
        {
            InitializeComponent();

            DataContext = this;

            SelectableTextBlock_Content.Text = message;
            TextBlock_Title.Text = title;

            switch (type)
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
            var clickedButton = sender as Button;

            if (clickedButton != null)
            {
                switch (clickedButton.Content)
                {
                    case Content_Yes:
                        Result = MessageBoxResult.Yes;
                        break;

                    case Content_No:
                        Result = MessageBoxResult.No;
                        break;
                }

                Close();
            }
        }

        private void Window_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape ||
                e.Key == Key.Enter)
            {
                Close();
            }
        }

        private void Chrome_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            BeginMoveDrag(e);
        }

        private void Button_Close_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
