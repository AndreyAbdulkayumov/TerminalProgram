using Avalonia.Controls;

namespace TerminalProgram.Views.ModbusClient.WriteFields;

public partial class MultipleRegisters : UserControl
{
    public MultipleRegisters()
    {
        InitializeComponent();
    }

    private void UppercaseTextBox_TextChanged(object sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        TextBox? textBox = sender as TextBox;

        if (textBox != null)
        {
            textBox.Text = textBox.Text?.ToUpper();
        }
    }
}