using Avalonia.Controls;

namespace TerminalProgram.Views.Protocols
{
    public partial class NoProtocol : UserControl
    {
        public NoProtocol()
        {
            InitializeComponent();

            ScrollViewer? e2 = ControlExtensions.FindControl<ScrollViewer>(TextBox_Test, "PART_ScrollViewer");
        }
                
        private void TextBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            TextBox? element = sender as TextBox;

            // ѕрокрутка текста в конец

            if (element != null)
            {
                // ѕохоже что CaretIndex реагирует только на изменение значени€,
                // поэтому когда Text достигает максимального количества символов
                // прокрутка не всегда происходит.
                element.CaretIndex = 0;
                element.CaretIndex = element.Text == null ? 0 : element.Text.Length;

                //ScrollViewer_RX.ScrollToEnd();
            }
        }
    }
}
