using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Styling;
using MessageBox_Core;
using MessageBox_AvaloniaUI;
using System;
using System.Reactive.Linq;
using ViewModels.Settings;

namespace TerminalProgram.Views.Settings
{
    public partial class SettingsWindow : Window
    {
        private readonly ViewModel_Settings ViewModel;

        private readonly IMessageBox Message;

        public SettingsWindow()
        {
            InitializeComponent();

            Message = new MessageBox(MainWindow.Instance, "Терминальная программа");

            ViewModel = new ViewModel_Settings(
                Message.Show,
                Message.ShowYesNoDialog,
                GetFilePathEmpty,
                GetFileNameEmpty,
                Set_Dark_Theme,
                Set_Light_Theme
                );

            DataContext = ViewModel;
        }

        private async void Window_Loaded(object? sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.Command_Loaded.Execute();
            }
        }

        private async void Window_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    await ViewModel.Command_File_Save.Execute();
                    break;

                case Key.Escape:
                    this.Close();
                    break;
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

        private string GetFilePathEmpty(string WindowTitle)
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
