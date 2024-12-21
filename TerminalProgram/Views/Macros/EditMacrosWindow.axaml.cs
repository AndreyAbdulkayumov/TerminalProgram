using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ViewModels.Macros;

namespace TerminalProgram.Views.Macros;

public partial class EditMacrosWindow : Window
{
    private readonly CreateMacros_VM _viewModel;

    public EditMacrosWindow()
    {
        InitializeComponent();

        _viewModel = new CreateMacros_VM(Close);

        DataContext = _viewModel;
    }

    public object? GetData()
    {
        return _viewModel.Saved ? _viewModel.GetMacrosContent() : null;
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