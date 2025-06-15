using Avalonia.Controls;
using System;
using ViewModels.Macros.MacrosEdit.CommandEdit;

namespace CoreBus.Base.Views.Macros.EditMacros.EditCommandViews;

public partial class NoProtocolCommand : UserControl
{
    private readonly TextBox? _inputField_TX;

    private Func<string>? GetValidatedInput;

    public NoProtocolCommand()
    {
        InitializeComponent();

        _inputField_TX = this.FindControl<TextBox>("TextBox_TX");
    }

    private void TextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_inputField_TX != null && GetValidatedInput != null)
        {
            _inputField_TX.Text = GetValidatedInput();
        }
    }

    private void UserControl_DataContextChanged(object? sender, EventArgs e)
    {
        var normalMode_VM = DataContext as NoProtocolCommand_VM;

        if (normalMode_VM != null)
        {
            GetValidatedInput = normalMode_VM.GetValidatedString;
        }
    }
}