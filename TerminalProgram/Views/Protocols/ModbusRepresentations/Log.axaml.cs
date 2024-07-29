using Avalonia.Controls;

namespace TerminalProgram.Views.Protocols.ModbusRepresentations
{
    public partial class Log : UserControl
    {
        public Log()
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
