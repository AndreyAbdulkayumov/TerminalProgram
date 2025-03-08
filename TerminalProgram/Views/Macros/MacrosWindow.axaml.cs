using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace TerminalProgramBase.Views.Macros;

public partial class MacrosWindow : Window
{
    public static MacrosWindow? Instance { get; private set; }
    public static Grid? Workspace { get; private set; }

    private readonly Border? _resizeIcon;

    public MacrosWindow()
    {
        InitializeComponent();

        Instance = this;
        Workspace = this.FindControl<Grid>("Grid_Workspace");

        _resizeIcon = this.FindControl<Border>("Border_ResizeIcon");
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