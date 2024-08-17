using Avalonia.Controls;
using Avalonia.Threading;
using System.Collections.ObjectModel;
using ViewModels.MainWindow;
using ViewModels.MainWindow.Modbus;

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
    }
}
