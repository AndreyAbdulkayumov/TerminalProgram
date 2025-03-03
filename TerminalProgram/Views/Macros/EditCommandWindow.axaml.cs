using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MessageBox_AvaloniaUI;
using ViewModels.Macros.CommandEdit;
using ViewModels.Macros.DataTypes;

namespace TerminalProgramBase.Views.Macros;

public partial class EditCommandWindow : Window
{
    private readonly EditCommand_VM _viewModel;

    public EditCommandWindow(EditCommandParameters parameters)
    {
        InitializeComponent();

        _viewModel = new EditCommand_VM(
            parameters,
            Close, 
            new MessageBox(this)
            );

        DataContext = _viewModel;
    }

    public object? GetData()
    {
        return _viewModel.Saved ? _viewModel.GetCommandContent() : null;
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