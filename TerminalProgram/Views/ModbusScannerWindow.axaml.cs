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
            this.BeginMoveDrag(e);
        }

        private async void Button_Close_Click(object? sender, RoutedEventArgs e)
        {
            ModbusScanner_VM? Context = this.DataContext as ModbusScanner_VM;

            if (Context != null)
            {
                await Context.Close_EventHandler();
            }
                
            this.Close();
        }
    }
}
