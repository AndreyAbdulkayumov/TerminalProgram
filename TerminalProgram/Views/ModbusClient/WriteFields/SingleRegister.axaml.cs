using Avalonia.Controls;

namespace TerminalProgram.Views.ModbusClient.WriteFields;

public partial class SingleRegister : UserControl
{
    public SingleRegister()
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