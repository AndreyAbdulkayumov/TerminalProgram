using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MessageBox_AvaloniaUI;
using MessageBox_Core;
using ViewModels.Macros;

namespace TerminalProgram.Views.Macros;

public partial class MacrosWindow : Window
{
    private static bool _isOpen = false;

    private readonly IMessageBox _messageBox;

    public MacrosWindow()
    {
        InitializeComponent();

        _messageBox = new MessageBox(this, "Макросы");

        DataContext = new Macros_VM(_messageBox);
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