using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace TerminalProgram.Views;

public partial class MainWindow : Window
{
    public static Window? Instance { get; private set; }

    private CornerRadius WindowCornerRadius;
    private CornerRadius ButtonCloseRadius;

    public MainWindow()
    {
        Instance = this;

        InitializeComponent();
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
            Button_Close.CornerRadius = ButtonCloseRadius;
        }

        else
        {
            WindowCornerRadius = Border_Window.CornerRadius;
            ButtonCloseRadius = Button_Close.CornerRadius;

            this.WindowState = WindowState.Maximized;

            Border_Window.CornerRadius = new CornerRadius(0, 0, 0, 0);
            Button_Close.CornerRadius = new CornerRadius(0, 0, 0, 0);
        }
    }

    private void Button_Close_Click(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
