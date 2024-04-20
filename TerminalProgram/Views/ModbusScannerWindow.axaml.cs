using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace TerminalProgram.Views
{
    public partial class ModbusScannerWindow : Window
    {
        public ModbusScannerWindow()
        {
            InitializeComponent();
        }

        private void Chrome_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            this.BeginMoveDrag(e);
        }

        private void Button_Close_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
