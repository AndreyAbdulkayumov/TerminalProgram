using Avalonia.Controls;
using Avalonia.Threading;
using System.Collections.ObjectModel;
using ViewModels.ModbusClient;
using ViewModels.ModbusClient.DataTypes;

namespace TerminalProgram.Views.ModbusClient
{
    public partial class ModbusClient_View : UserControl
    {
        private ObservableCollection<ModbusDataDisplayed> _dataInDataGrid = new ObservableCollection<ModbusDataDisplayed>();

        public ModbusClient_View()
        {
            InitializeComponent();

            DataGrid_ModbusData.ItemsSource = _dataInDataGrid;

            ModbusClient_VM.AddDataOnTable += ViewModel_ModbusClient_AddDataOnTable;
        }

        private async void ViewModel_ModbusClient_AddDataOnTable(object? sender, ModbusDataDisplayed? e)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
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
