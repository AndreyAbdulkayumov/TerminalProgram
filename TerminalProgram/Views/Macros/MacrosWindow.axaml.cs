using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MessageBox_AvaloniaUI;
using MessageBox_Core;
using ViewModels.Macros;

namespace TerminalProgram.Views.Macros;

public partial class MacrosWindow : Window
{
    private readonly IMessageBox _messageBox;

    public MacrosWindow()
    {
        InitializeComponent();

        _messageBox = new MessageBox(this, "Макросы");

        DataContext = new Macros_VM(_messageBox);
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