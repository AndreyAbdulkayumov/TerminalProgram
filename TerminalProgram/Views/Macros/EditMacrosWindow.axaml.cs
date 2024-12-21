using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ViewModels.Macros;

namespace TerminalProgram.Views.Macros;

public partial class EditMacrosWindow : Window
{
    public EditMacrosWindow()
    {
        InitializeComponent();

        DataContext = new CreateMacros_VM();
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