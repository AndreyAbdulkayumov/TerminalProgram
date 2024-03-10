using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Styling;
using MessageBox_Core;
using System;
using ViewModels.Settings;

namespace TerminalProgram.Views.Settings
{
    public partial class SettingsWindow : Window
    {
        private readonly ViewModel_Settings ViewModel;

        public SettingsWindow()
        {
            InitializeComponent();

            ViewModel = new ViewModel_Settings(
                MessageEmpty,
                MessageDialogEmpty,
                GetFilePathEmpty,
                GetFileNameEmpty,
                Set_Dark_Theme,
                Set_Light_Theme
                );

            DataContext = ViewModel;
        }

        private void Chrome_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            this.BeginMoveDrag(e);
        }

        private void Button_Close_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MessageEmpty(string Message, MessageType Type)
        {

        }

        private bool MessageDialogEmpty(string Message, MessageType Type)
        {
            return true;
        }

        private string GetFilePathEmpty(string Path)
        {
            return string.Empty;
        }

        private string GetFileNameEmpty()
        {
            return string.Empty;
        }

        private void Set_Dark_Theme()
        {
            if (Application.Current != null)
            {
                Application.Current.Resources.MergedDictionaries.Clear();

                Application.Current.Resources.MergedDictionaries.Add(new ResourceInclude(
                    new Uri("avares://TerminalProgram/Themes/Dark.axaml"))
                {
                    Source = new Uri("avares://TerminalProgram/Themes/Dark.axaml")
                });

                Application.Current.RequestedThemeVariant =
                    new Avalonia.Styling.ThemeVariant("Dark", Application.Current.ActualThemeVariant);
            }
        }

        private void Set_Light_Theme()
        {
            if (Application.Current != null)
            {
                Application.Current.Resources.MergedDictionaries.Clear();

                Application.Current.Resources.MergedDictionaries.Add(new ResourceInclude(
                    new Uri("avares://TerminalProgram/Themes/Light.axaml"))
                {
                    Source = new Uri("avares://TerminalProgram/Themes/Light.axaml")
                });

                Application.Current.RequestedThemeVariant =
                    new Avalonia.Styling.ThemeVariant("Light", Application.Current.ActualThemeVariant);
            }
        }
    }
}
