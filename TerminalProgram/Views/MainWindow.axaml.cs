using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Threading;
using MessageBox_AvaloniaUI;
using MessageBox_Core;
using TerminalProgram.Views.Settings;
using TerminalProgram.Views.Macros;
using ViewModels;


namespace TerminalProgram.Views;

public partial class MainWindow : Window
{
    private readonly CommonUI_VM ViewModel;

    private readonly IMessageBox Message;

    private readonly double WorkspaceOpacity_Default;
    private const double WorkspaceOpacity_OpenChildWindow = 0.15;

    private readonly Border? _resizeIcon;

    public MainWindow()
    {
        InitializeComponent();

        WorkspaceOpacity_Default = Grid_Workspace.Opacity;

        _resizeIcon = this.FindControl<Border>("Border_ResizeIcon");

        Message = new MessageBox(this, "Терминальная программа");

        ViewModel = new CommonUI_VM(
                AboutWindow.GetAppVersion(),
                RunInUIThread,
                OpenWindow_ModbusScanner,
                Message,
                Set_Dark_Theme,
                Set_Light_Theme,
                CopyToClipboard
                );

        DataContext = ViewModel;
    }

    private async Task RunInUIThread(Action RunnedAction)
    {
        await Dispatcher.UIThread.InvokeAsync(RunnedAction);
    }

    private async Task OpenWindow_ModbusScanner()
    {
        await OpenWindowWithDimmer(async () =>
        {
            var window = new ModbusScannerWindow();

            await window.ShowDialog(this);
        });
    }

    private async Task CopyToClipboard(string Data)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        var dataObject = new DataObject();

        dataObject.Set(DataFormats.Text, Data);

        if (clipboard != null)
        {
            await clipboard.SetDataObjectAsync(dataObject);
        }        
    }

    private void Set_Dark_Theme()
    {
        if (Application.Current != null)
        {
            Application.Current.Resources.MergedDictionaries.Clear();

            Application.Current.Resources.MergedDictionaries.Add(new ResourceInclude(
                new Uri("avares://AppDesign/Themes/Dark.axaml"))
            {
                Source = new Uri("avares://AppDesign/Themes/Dark.axaml")
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
                new Uri("avares://AppDesign/Themes/Light.axaml"))
            {
                Source = new Uri("avares://AppDesign/Themes/Light.axaml")
            });

            Application.Current.RequestedThemeVariant =
                new Avalonia.Styling.ThemeVariant("Light", Application.Current.ActualThemeVariant);
        }
    }

    /********************************************************/
    //
    //  События окна
    //
    /********************************************************/

    private async void Window_Loaded(object? sender, RoutedEventArgs e)
    {
        await ViewModel.MainWindowLoadedHandler();
    }

    private async void Window_Closing(object? sender, WindowClosingEventArgs e)
    {
        try
        {
            await ViewModel.Command_Closing.Execute();
        }
        
        catch (Exception error)
        {
            Message.Show("Ошибка закрытия приложения.\n\n" + error.Message, MessageType.Error);
        }
    }

    private void Chrome_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    private void Button_Minimize_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Button_Maximize_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        if (_resizeIcon != null)
        {
            _resizeIcon.IsVisible = WindowState == WindowState.Normal ? true : false;
        }
    }

    private void Button_Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ResizeIcon_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Cursor = new(StandardCursorType.BottomRightCorner);
        BeginResizeDrag(WindowEdge.SouthEast, e);
        Cursor = new(StandardCursorType.Arrow);
    }

    /********************************************************/
    //
    //  Обработчики кнопок
    //
    /********************************************************/

    private async void Button_OpenSettings_Click(object? sender, RoutedEventArgs e)
    {
        await OpenWindowWithDimmer(async () =>
        {
            var window = new SettingsWindow(Set_Dark_Theme, Set_Light_Theme);

            await window.ShowDialog(this);

            await ViewModel.Command_UpdatePresets.Execute();
        });
    }

    private async void Button_About_Click(object? sender, RoutedEventArgs e)
    {
        await OpenWindowWithDimmer(async () =>
        {
            var window = new AboutWindow();

            await window.ShowDialog(this);
        });
    }

    private void Button_Macros_Click(object? sender, RoutedEventArgs e)
    {
        var window = new MacrosWindow();

        window.Show();
    }

    /********************************************************/
    //
    //  Служебный функционал
    //
    /********************************************************/

    private async Task OpenWindowWithDimmer(Func<Task> OpenAction)
    {
        await Dispatcher.UIThread.Invoke(async () =>
        {
            Grid_Workspace.Opacity = WorkspaceOpacity_OpenChildWindow;

            await OpenAction();

            Grid_Workspace.Opacity = WorkspaceOpacity_Default;
        });
    }
}
