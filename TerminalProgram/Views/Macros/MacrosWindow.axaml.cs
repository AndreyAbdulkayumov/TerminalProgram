using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MessageBox_AvaloniaUI;
using MessageBox_Core;
using System.Threading.Tasks;
using ViewModels.Macros;

namespace TerminalProgram.Views.Macros;

public partial class MacrosWindow : Window
{
    private static bool _isOpen = false;

    private readonly IMessageBox _messageBox;

    private readonly Macros_VM _viewModel;

    private readonly Border? _resizeIcon;

    public MacrosWindow()
    {
        InitializeComponent();

        _messageBox = new MessageBox(this, "Макросы");

        _viewModel = new Macros_VM(_messageBox, OpenCreateMacrosWindow);

        _resizeIcon = this.FindControl<Border>("Border_ResizeIcon");

        DataContext = _viewModel;
    }

    private async Task OpenCreateMacrosWindow()
    {
        await MainWindow.OpenWindowWithDimmer(async () =>
        {
            var window = new EditMacrosWindow();

            await window.ShowDialog(this);
        },
        Grid_Workspace);
    }

    /// <summary>
    /// Использовать для открытия окна в единственном экземпляре.
    /// </summary>
    public static void ShowWindow(Window owner)
    {
        if (_isOpen)
        {
            return;
        }

        var window = new MacrosWindow();

        window.Show(owner);

        _isOpen = true;
    }

    private void Window_Closing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
    {
        _isOpen = false;
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
}