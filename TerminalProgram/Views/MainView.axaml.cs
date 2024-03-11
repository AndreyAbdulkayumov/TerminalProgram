using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using MessageBox_Core;
using System;
using System.Reactive.Linq;
using TerminalProgram.Views.Settings;
using ViewModels.MainWindow;

namespace TerminalProgram.Views;

public partial class MainView : UserControl
{
    private readonly ViewModel_CommonUI ViewModel;

    public MainView()
    {
        InitializeComponent();

        ViewModel = new ViewModel_CommonUI(
                //MessageView.Show,
                MessageEmpty,
                Select_AvailablePresetFiles,
                "Unknown",
                "Dark",
                "Dark",
                "Light",
                Set_Dark_Theme,
                Set_Light_Theme
                );

        DataContext = ViewModel;
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

            //Application.Current.RequestedThemeVariant =
            //    new Avalonia.Styling.ThemeVariant("Dark", Application.Current.ActualThemeVariant);
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

            //Application.Current.RequestedThemeVariant =
            //    new Avalonia.Styling.ThemeVariant("Light", Application.Current.ActualThemeVariant);
        }
    }

    private async void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await ViewModel.Command_UpdatePresets.Execute();
    }

    private void MessageEmpty(string Message, MessageType Type)
    {

    }

    private string? Select_AvailablePresetFiles(string[] Files)
    {
        //ComboBoxWindow window = new ComboBoxWindow(Files)
        //{
        //    Owner = this
        //};

        //window.ShowDialog();

        //if (window.SelectedDocumentPath != String.Empty)
        //{
        //    return window.SelectedDocumentPath;
        //}

        //else
        //{
        //    Application.Current.Shutdown();
        //    return null;
        //}

        return "Unknown";
    }

    private async void Button_OpenSettings_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (MainWindow.Instance == null)
        {
            return;
        }

        SettingsWindow window = new SettingsWindow();    

        await window.ShowDialog(MainWindow.Instance);

        await ViewModel.Command_UpdatePresets.Execute();
    }

    private async void Button_About_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (MainWindow.Instance == null)
        {
            return;
        }

        AboutWindow window = new AboutWindow();

        await window.ShowDialog(MainWindow.Instance);
    }

}
