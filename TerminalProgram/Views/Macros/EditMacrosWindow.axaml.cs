using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MessageBox_AvaloniaUI;
using ViewModels.Macros;
using ViewModels.Macros.DataTypes;

namespace TerminalProgram.Views.Macros;

public partial class EditMacrosWindow : Window
{
    private readonly EditMacros_VM _viewModel;

    public EditMacrosWindow(EditMacrosParameters parameters)
    {
        InitializeComponent();

        _viewModel = new EditMacros_VM(
            parameters,
            Close, 
            new MessageBox(this, "Макросы")
            );

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