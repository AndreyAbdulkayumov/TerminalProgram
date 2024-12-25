using Avalonia.Controls;
using ViewModels.Macros;

namespace TerminalProgram.Views.Macros;

public partial class MacrosItem : UserControl
{
    private readonly Button? _button_Settings, _button_Delete;

    public MacrosItem()
    {
        InitializeComponent();

        _button_Settings = this.FindControl<Button>("Button_Settings");
        _button_Delete = this.FindControl<Button>("Button_Delete");

        ViewServiceButtons(false);
    }

    private async void Border_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        var viewModel = DataContext as MacrosViewItem_VM;

        if (viewModel != null)
        {
            await viewModel.MacrosAction();
        }

        ViewServiceButtons(true);
    }

    private void Border_PointerEntered(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        ViewServiceButtons(true);
    }

    private void Border_PointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        ViewServiceButtons(false);
    }

    private void ViewServiceButtons(bool value)
    {
        if (_button_Settings != null)
        {
            _button_Settings.IsVisible = value;
        }

        if (_button_Delete != null)
        {
            _button_Delete.IsVisible = value;
        }
    }
}