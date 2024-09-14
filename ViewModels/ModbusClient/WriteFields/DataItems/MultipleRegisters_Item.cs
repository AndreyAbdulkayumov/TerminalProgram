using ReactiveUI;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reactive;

namespace ViewModels.ModbusClient.WriteFields.DataItems
{
    public class MultipleRegisters_Item : ModbusDataFormatter
    {
        private string? _startAddressAddition;

        public string? StartAddressAddition
        {
            get => _startAddressAddition;
            set
            {
                this.RaiseAndSetIfChanged(ref _startAddressAddition, value);
            }
        }

        private UInt16 _data = 0;

        public UInt16 Data
        {
            get => _data;
            set
            {
                _data = value;
                //ViewData = ConvertNumberToString(value, DataFormat);
            }
        }

        private string? _viewData;

        public override string? ViewData
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
            DataFormatName_dec, DataFormatName_hex, DataFormatName_bin
        };

        public ObservableCollection<string> FormatItems
        {
            get => _formatItems;
        }

        private string? _selectedDataFormat;

        public string? SelectedDataFormat
        {
            get => _selectedDataFormat;
            set => SetDataFormat(value);
        }

        public readonly Guid Id;
        public readonly bool CanRemove;

        public ReactiveCommand<Unit, Unit>? Command_RemoveItem { get; set; }


        public MultipleRegisters_Item(
            bool canRemove,
            int startAddressAddition,
            ushort data, string dataFormat,
            Action<Guid> removeItemHandler)
        {
            Id = Guid.NewGuid();

            CanRemove = canRemove;

            StartAddressAddition = $"+{startAddressAddition}";

            SelectedDataFormat = dataFormat;
            //SelectedDataFormat = "bin";

            Data = data;
            ViewData = ConvertNumberToString(data, DataFormat);

            if (canRemove)
            {
                Command_RemoveItem = ReactiveCommand.Create(() =>
                {
                    removeItemHandler?.Invoke(Id);
                });
            }

            this.WhenAnyValue(x => x.ViewData)
                .WhereNotNull()
                .Subscribe(x =>
                {
                    Data = ConvertStringToNumber(x, DataFormat);
                    ViewData = x;
                });
        }

        public override void SetDataFormat(string? format)
        {
            switch (format)
            {
                case DataFormatName_dec:
                    DataFormat = NumberStyles.Number;
                    break;

                case DataFormatName_hex:
                    DataFormat = NumberStyles.HexNumber;
                    break;

                case DataFormatName_bin:
                    DataFormat = NumberStyles.BinaryNumber;
                    break;

                default:
                    throw new Exception("Неподдерживаемый формат числа: " + format);
            }

            _selectedDataFormat = format;

            ViewData = ConvertNumberToString(Data, DataFormat);
        }
    }
}
