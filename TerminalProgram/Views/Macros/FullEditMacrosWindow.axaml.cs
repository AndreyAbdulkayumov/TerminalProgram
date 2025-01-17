using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MessageBox_AvaloniaUI;
using ViewModels.Macros.MacrosEdit;

namespace TerminalProgram.Views.Macros;

public partial class FullEditMacrosWindow : Window
{
    public FullEditMacrosWindow()
    {
        InitializeComponent();

        DataContext = new FullEditMacros_VM(new MessageBox(this, "Макросы"));
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