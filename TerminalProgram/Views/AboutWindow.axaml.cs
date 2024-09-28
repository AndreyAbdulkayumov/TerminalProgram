using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Reflection;

namespace TerminalProgram.Views
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            int numberOfChars;

            char[] appVersion_Chars = new char[20];

            if (Assembly.GetExecutingAssembly().GetName().Version?.TryFormat(appVersion_Chars, 3, out numberOfChars) == true)
            {
                TextBlock_App_Version.Text = new string(appVersion_Chars, 0, numberOfChars);
            }

            char[] GUIVersion_Chars = new char[20];

            if (typeof(AvaloniaObject).Assembly.GetName().Version?.TryFormat(GUIVersion_Chars, 3, out numberOfChars) == true)
            {
                TextBlock_GUI_Version.Text = new string(GUIVersion_Chars, 0, numberOfChars);
            }

            TextBlock_Runtime_Version.Text = Environment.Version.ToString();
        }

        private void Chrome_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            BeginMoveDrag(e);
        }

        private void Button_Close_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Space || e.Key == Key.Enter)
            {
                Close();
            }
        }

        private void Button_CheckUpdate_Click(object? sender, RoutedEventArgs e)
        {

        }

        private void Button_Donate_Click(object? sender, RoutedEventArgs e)
        {

        }
    }
}
