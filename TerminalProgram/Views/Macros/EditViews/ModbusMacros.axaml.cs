using Avalonia.Controls;

namespace TerminalProgramBase.Views.Macros.EditViews;

public partial class ModbusMacros : UserControl
{
    public ModbusMacros()
    {
        InitializeComponent();
    }

    private void UppercaseTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        TextBox? textBox = sender as TextBox;

        if (textBox != null)
        {
            textBox.Text = textBox.Text?.ToUpper();
        }
    }
}