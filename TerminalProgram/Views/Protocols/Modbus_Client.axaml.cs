using Avalonia.Controls;
using Avalonia.Threading;
using System.Collections.ObjectModel;
using ViewModels.ModbusClient;
using ViewModels.ModbusClient.DataTypes;

namespace TerminalProgram.Views.Protocols
{
    public partial class Modbus_Client : UserControl
    {
        private ObservableCollection<ModbusDataDisplayed> _dataInDataGrid = new ObservableCollection<ModbusDataDisplayed>();

        public Modbus_Client()
        {
            InitializeComponent();

            DataGrid_ModbusData.ItemsSource = _dataInDataGrid;

            ModbusClient_VM.AddDataOnTable += ViewModel_ModbusClient_AddDataOnTable;
        }

        private void ViewModel_ModbusClient_AddDataOnTable(object? sender, ModbusDataDisplayed? e)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                if (e != null)
                {
                    _dataInDataGrid.Add(e);
                    DataGrid_ModbusData.ScrollIntoView(e, null);
                    return;
                }

                _dataInDataGrid.Clear();
            });            
        }
    }
}
