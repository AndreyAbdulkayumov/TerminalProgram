using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace TerminalProgram.Views;

public partial class MainWindow : Window
{
    public static Window? Instance { get; private set; }

    private CornerRadius WindowCornerRadius;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object? sender, RoutedEventArgs e)
    {
        Instance = this;
    }

    private void Chrome_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        this.BeginMoveDrag(e);
    }

    private void Button_Minimize_Click(object? sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void Button_Maximize_Click(object? sender, RoutedEventArgs e)
    {
        if (this.WindowState == WindowState.Maximized)
        {
            this.WindowState = WindowState.Normal;
            Border_Window.CornerRadius = WindowCornerRadius;
        }

        else
        {
            WindowCornerRadius = Border_Window.CornerRadius;
            this.WindowState = WindowState.Maximized;
            Border_Window.CornerRadius = new CornerRadius(0, 0, 0, 0);
        }
    }

    private void Button_Close_Click(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
