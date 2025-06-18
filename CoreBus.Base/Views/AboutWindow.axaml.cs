using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace CoreBus.Base.Views;

public partial class AboutWindow : Window
{
    public static AboutWindow? Instance { get; private set; }

    public AboutWindow()
    {
        InitializeComponent();

        Instance = this;
    }

    private void Chrome_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    private void Button_Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape || e.Key == Key.Space || e.Key == Key.Enter)
        {
            Close();
        }
    }
}
