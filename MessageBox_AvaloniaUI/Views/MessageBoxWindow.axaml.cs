using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MessageBox_AvaloniaUI.ViewModels;
using MessageBox_Core;
using System;
using System.Threading.Tasks;

namespace MessageBox_AvaloniaUI.Views
{
    public partial class MessageBoxWindow : Window
    {
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.Default;

        public MessageBoxWindow(string message, string title, MessageType messageType, MessageBoxToolType toolType, string? appVersion, Exception? error = null)
        {
            InitializeComponent();

            DataContext = new MessageBox_VM(OpenErrorReport, CopyToClipboard, message, title, messageType, toolType, appVersion, error);
        }

        private void OpenErrorReport(string errorReport)
        {
            var window = new ViewErrorWindow(errorReport);

            window.Show(this);
        }

        private async Task CopyToClipboard(string data)
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            var dataObject = new DataObject();

            dataObject.Set(DataFormats.Text, data);

            if (clipboard != null)
            {
                await clipboard.SetDataObjectAsync(dataObject);
            }
        }

        private void Button_Click(object? sender, RoutedEventArgs e)
        {
            var clickedButton = sender as Button;

            if (clickedButton != null)
            {
                switch (clickedButton.Content)
                {
                    case MessageBox_VM.Content_Yes:
                        Result = MessageBoxResult.Yes;
                        break;

                    case MessageBox_VM.Content_No:
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
