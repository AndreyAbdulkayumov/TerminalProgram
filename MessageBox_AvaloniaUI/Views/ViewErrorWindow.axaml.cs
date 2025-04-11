using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace MessageBox_AvaloniaUI;

public partial class ViewErrorWindow : Window
{
    public ViewErrorWindow(string errorReport)
    {
        InitializeComponent();

        TextBlock_ErrorReport.Text = errorReport;
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