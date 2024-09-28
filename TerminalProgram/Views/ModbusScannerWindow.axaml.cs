using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ViewModels.ModbusClient;

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
            BeginMoveDrag(e);
        }

        private async void Button_Close_Click(object? sender, RoutedEventArgs e)
        {
            var context = DataContext as ModbusScanner_VM;

            if (context != null)
            {
                await context.Close_EventHandler();
            }
                
            Close();
        }
    }
}
