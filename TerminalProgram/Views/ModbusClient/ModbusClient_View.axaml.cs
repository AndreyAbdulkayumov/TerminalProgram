using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ViewModels.ModbusClient;
using ViewModels.ModbusClient.DataTypes;

namespace TerminalProgram.Views.ModbusClient
{
    public class ModbusDataDisplayedForTable
    {
        public ushort OperationID { get; set; }
        public string? FuncNumber { get; set; }
        public string? ViewAddress { get; set; }
        public string? ViewData { get; set; }
        public SolidColorBrush? RowBackground { get; set; }
    }

    public partial class ModbusClient_View : UserControl
    {
        private const string ResourceKey_DataGrid_Color_RowBackground_Selected = "DataGrid_Color_RowBackground_Selected";
        private const string ResourceKey_DataGrid_Color_RowBackground = "DataGrid_Color_RowBackground";
        private const string ResourceKey_DataGrid_Color_AlternatingRowBackground = "DataGrid_Color_AlternatingRowBackground";

        private ObservableCollection<ModbusDataDisplayedForTable> _dataInDataGrid = new ObservableCollection<ModbusDataDisplayedForTable>();

        private Border? _selectedBorder;
        private SolidColorBrush? _selectedBorder_InitColor;

        public ModbusClient_View()
        {
            InitializeComponent();

            var itemsControl = this.FindControl<ItemsControl>("ItemsControl_ModbusData");

            if (itemsControl != null)
            {
                itemsControl.ItemsSource = _dataInDataGrid;
            }

            ModbusClient_VM.AddDataOnTable += ViewModel_ModbusClient_AddDataOnTable;
        }

        private void Border_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            Border? border = sender as Border;

            if (border != null)
            {
                if (_selectedBorder != null)
                {
                    _selectedBorder.Background = _selectedBorder_InitColor;
                    _selectedBorder.BorderBrush = _selectedBorder_InitColor;
                }

                _selectedBorder = border;
                _selectedBorder_InitColor = (SolidColorBrush?)border.Background;

                if (this.TryFindResource(ResourceKey_DataGrid_Color_RowBackground_Selected, out object? rowSelectedBackground))
                {
                    border.Background = (IBrush?)rowSelectedBackground;
                }

                border.BorderBrush = Brushes.Black;
            }
        }

        private async void ViewModel_ModbusClient_AddDataOnTable(object? sender, ModbusDataDisplayed? e)
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (e != null)
                {
                    string resourceKey = _dataInDataGrid.Count % 2 != 0 ? ResourceKey_DataGrid_Color_AlternatingRowBackground : ResourceKey_DataGrid_Color_RowBackground;

                    if (this.TryFindResource(resourceKey, out object? rowBackground) == false)
                    {
                        rowBackground = Brushes.Transparent;
                    }

                    _dataInDataGrid.Add(new ModbusDataDisplayedForTable()
                    {
                        OperationID = e.OperationID,
                        FuncNumber = e.FuncNumber,
                        ViewAddress = e.ViewAddress,
                        ViewData = e.ViewData,
                        RowBackground = rowBackground as SolidColorBrush
                    });

                    await ScrollToEnd();

                    return;
                }

                _dataInDataGrid.Clear();
            });            
        }

        public async Task ScrollToEnd()
        {
            var listView = this.FindControl<ScrollViewer>("ScrollViewer_ModbusData");

            if (listView != null)
            {
                await Task.Delay(10);  // Элемент отрисовывается не сразу. Нужно немного подождать, чтобы скрол точно произошел к последнему элементу 

                listView.ScrollToEnd();
            }
        }
    }
}
