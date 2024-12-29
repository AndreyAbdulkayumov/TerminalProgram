using Avalonia.Controls;
using System;
using ViewModels.Macros.MacrosEdit;

namespace TerminalProgram.Views.Macros.EditViews;

public partial class NoProtocolMacros : UserControl
{
    private readonly TextBox? _inputField_TX;

    private Func<string>? GetValidatedInput;

    public NoProtocolMacros()
    {
        InitializeComponent();

        _inputField_TX = this.FindControl<TextBox>("TextBox_TX");
    }

    private void TextBox_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        if (_inputField_TX != null && GetValidatedInput != null)
        {
            _inputField_TX.Text = GetValidatedInput();
        }
    }

    private void UserControl_DataContextChanged(object? sender, System.EventArgs e)
    {
        var normalMode_VM = DataContext as NoProtocolMacros_VM;

        if (normalMode_VM != null)
        {
            GetValidatedInput = normalMode_VM.GetValidatedString;
        }
    }
}