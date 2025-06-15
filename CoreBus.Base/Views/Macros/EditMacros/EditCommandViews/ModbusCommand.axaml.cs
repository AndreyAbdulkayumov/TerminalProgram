using Avalonia.Controls;

namespace CoreBus.Base.Views.Macros.EditMacros.EditCommandViews;

public partial class ModbusCommand : UserControl
{
    public ModbusCommand()
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