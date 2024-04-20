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

            int NumberOfChars;

            char[] AppVersion_Chars = new char[20];

            if (Assembly.GetExecutingAssembly().GetName().Version?.TryFormat(AppVersion_Chars, 3, out NumberOfChars) == true)
            {
                TextBlock_App_Version.Text = new string(AppVersion_Chars, 0, NumberOfChars);
            }

            char[] GUIVersion_Chars = new char[20];

            if (typeof(AvaloniaObject).Assembly.GetName().Version?.TryFormat(GUIVersion_Chars, 3, out NumberOfChars) == true)
            {
                TextBlock_GUI_Version.Text = new string(GUIVersion_Chars, 0, NumberOfChars);
            }

            TextBlock_Runtime_Version.Text = Environment.Version.ToString();
        }

        private void Chrome_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            this.BeginMoveDrag(e);
        }

        private void Button_Close_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Space || e.Key == Key.Enter)
            {
                this.Close();
            }
        }
    }
}
