using Avalonia.Controls;

namespace TerminalProgram.Views.Protocols
{
    public partial class Modbus_Client : UserControl
    {
        public Modbus_Client()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
        {
            TextBox? LogText = sender as TextBox;

            if (LogText != null)
            {
                LogText.CaretIndex = 0;
                LogText.CaretIndex = LogText.Text == null ? 0 : LogText.Text.Length;
            }
        }
    }
}
