using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace TerminalProgramBase.Views.Macros;

public partial class EditMacrosWindow : Window
{
    public static EditMacrosWindow? Instance { get; private set; }

    public EditMacrosWindow()
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
}