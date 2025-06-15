using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace MessageBox.AvaloniaUI;

public partial class ViewErrorWindow : Window
{
    public ViewErrorWindow()
    {
        InitializeComponent();
    }

    public void SetErrorReport(string errorReport)
    {
        TextBlock_ErrorReport.Text = errorReport;
    }

    private void Window_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape ||
            e.Key == Key.Enter)
        {
            Close();
        }
    }

    private void Chrome_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
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