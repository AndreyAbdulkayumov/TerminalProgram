using ReactiveUI;
using System.Collections.ObjectModel;
using System.Globalization;

namespace ViewModels.ModbusClient.WriteFields
{
    public class SingleRegister_VM : ReactiveObject, IWriteField_VM
    {
        private ushort _data = 0;

        public ushort Data
        {
            get => _data;
            set
            {
                _data = value;
                //ViewData = ConvertNumberToString(value, DataFormat);
            }
        }

        private string? _viewData;

        public string? ViewData
        {
            get => _viewData;
            set
            {
                //Data = ConvertStringToNumber(value, DataFormat);
                this.RaiseAndSetIfChanged(ref _viewData, value);
            }
        }

        public NumberStyles DataFormat { get; private set; }

        private ObservableCollection<string> _formatItems = new ObservableCollection<string>()
        {
            "dec", "hex", "bin"
        };

        public ObservableCollection<string> FormatItems
        {
            get => _formatItems;
        }

        public SingleRegister_VM()
        {

        }

        public UInt16[] GetData()
        {
            return [Data];
        }
    }
}
