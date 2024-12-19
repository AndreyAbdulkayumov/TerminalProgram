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

    public MacrosWindow()
    {
        InitializeComponent();

        _messageBox = new MessageBox(this, "�������");

        _viewModel = new Macros_VM(_messageBox, OpenCreateMacrosWindow);

        DataContext = _viewModel;
    }

    private async Task OpenCreateMacrosWindow()
    {
        await MainWindow.OpenWindowWithDimmer(async () =>
        {
            var window = new CreateMacrosWindow();

            await window.ShowDialog(this);
        },
        Grid_Workspace);
    }

    /// <summary>
    /// ������������ ��� �������� ���� � ������������ ����������.
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