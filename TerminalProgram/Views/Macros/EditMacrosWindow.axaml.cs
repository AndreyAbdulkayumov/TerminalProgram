using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace TerminalProgramBase.Views.Macros;

public partial class EditMacrosWindow : Window
{
    public EditMacrosWindow()
    {
        InitializeComponent();
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