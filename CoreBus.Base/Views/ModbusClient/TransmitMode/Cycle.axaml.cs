using Avalonia.Controls;

namespace CoreBus.Base.Views.ModbusClient.TransmitMode;

public partial class Cycle : UserControl
{
    public Cycle()
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
