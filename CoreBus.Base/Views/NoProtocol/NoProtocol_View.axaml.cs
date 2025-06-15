using Avalonia.Controls;

namespace CoreBus.Base.Views.NoProtocol;

public partial class NoProtocol_View : UserControl
{
    public NoProtocol_View()
    {
        InitializeComponent();
    }

    private void TextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        var element = sender as TextBox;

        // ѕрокрутка текста в конец

        if (element != null)
        {
            // ѕохоже что CaretIndex реагирует только на изменение значени€,
            // поэтому когда Text достигает максимального количества символов
            // прокрутка не всегда происходит.
            element.CaretIndex = 0;
            element.CaretIndex = element.Text == null ? 0 : element.Text.Length;
        }
    }
}
