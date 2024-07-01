using Avalonia.Controls;
using Avalonia.Threading;
using System.Collections.ObjectModel;
using ViewModels.MainWindow;

namespace TerminalProgram.Views.Protocols
{
    public partial class Modbus_Client : UserControl
    {
        private ObservableCollection<ModbusDataDisplayed> DataInDataGrid = new ObservableCollection<ModbusDataDisplayed>();

        public Modbus_Client()
        {
            InitializeComponent();

            DataGrid_ModbusData.ItemsSource = DataInDataGrid;

            ViewModel_ModbusClient.AddDataOnTable += ViewModel_ModbusClient_AddDataOnTable;
        }

        private void ViewModel_ModbusClient_AddDataOnTable(object? sender, ModbusDataDisplayed? e)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                if (e != null)
                {
                    DataInDataGrid.Add(e);
                    DataGrid_ModbusData.ScrollIntoView(e, null);
                    return;
                }

                DataInDataGrid.Clear();
            });            
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
