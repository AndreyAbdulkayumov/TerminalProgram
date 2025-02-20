using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MessageBox_AvaloniaUI;
using System.Threading.Tasks;
using ViewModels.Macros.DataTypes;
using ViewModels.Macros.MacrosEdit;

namespace TerminalProgram.Views.Macros;

public partial class EditMacrosWindow : Window
{
    private readonly EditMacros_VM _viewModel;

    public EditMacrosWindow(object? macrosParameters)
    {
        InitializeComponent();

        _viewModel = new EditMacros_VM(macrosParameters, OpenEditCommandWindow, Close, new MessageBox(this, "Макросы"));

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

    private async Task<object?> OpenEditCommandWindow(EditCommandParameters parameters)
    {
        var window = new EditCommandWindow(parameters);

        await window.ShowDialog(this);

        return window.GetData();
    }
}