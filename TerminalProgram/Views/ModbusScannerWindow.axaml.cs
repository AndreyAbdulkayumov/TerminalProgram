using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ViewModels.MainWindow;

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

        private async void Button_Close_Click(object? sender, RoutedEventArgs e)
        {
            ViewModel_ModbusScanner? Context = this.DataContext as ViewModel_ModbusScanner;

            if (Context != null)
            {
                await Context.Close_EventHandler();
            }
                
            this.Close();
        }
    }
}
