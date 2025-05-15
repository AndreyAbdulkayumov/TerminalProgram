using Avalonia.Controls;

namespace TerminalProgramBase.Views.Macros.EditMacros;

public partial class CommonSlaveIDField : UserControl
{
    public CommonSlaveIDField()
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