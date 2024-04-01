using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml.Styling;
using MessageBox_AvaloniaUI;
using MessageBox_Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TerminalProgram.Views.Settings;
using ViewModels.MainWindow;

namespace TerminalProgram.Views;

public partial class MainView : UserControl
{
    private readonly ViewModel_CommonUI ViewModel;

    private readonly MessageBox mb;

    public MainView()
    {
        InitializeComponent();

        mb = new MessageBox(MainWindow.Instance, "Терминальная программа");

        ViewModel = new ViewModel_CommonUI(
                mb.Show,
                Select_AvailablePresetFiles,
                "Unknown",
                "Dark",
                "Dark",
                "Light",
                Set_Dark_Theme,
                Set_Light_Theme,
                CopyToClipboard
                );

        DataContext = ViewModel;
    }

    private async Task CopyToClipboard(string Data)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        var dataObject = new DataObject();

        dataObject.Set(DataFormats.Text, Data);

        await clipboard.SetDataObjectAsync(dataObject);
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
