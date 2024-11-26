using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MessageBox_AvaloniaUI;
using MessageBox_Core;
using ViewModels.ModbusClient;

namespace TerminalProgram.Views
{
    public partial class ModbusScannerWindow : Window
    {
        private readonly IMessageBox Message;

        public ModbusScannerWindow()
        {
            InitializeComponent();

            Message = new MessageBox(this, "Терминальная программа");

            DataContext = new ModbusScanner_VM(Message);
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
