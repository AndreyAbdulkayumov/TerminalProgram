using ReactiveUI;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reactive;

namespace ViewModels.ModbusClient
{
    public class ModbusClient_WriteData_VM : ReactiveObject
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
                Data = ConvertStringToNumber(value, DataFormat);
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

        private string? _selectedDataFormat;

        public string? SelectedDataFormat
        {
            get => _selectedDataFormat;
            set => SetNumberFormat(value);
        }

        public readonly Guid Id;
        public readonly bool CanRemove;

        public ReactiveCommand<Unit, Unit>? Command_RemoveItem { get; set; }


        public ModbusClient_WriteData_VM(
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
        }

        private void SetNumberFormat(string? format)
        {
            switch (format)
            {
                case "dec":
                    DataFormat = NumberStyles.Number;
                    break;

                case "hex":
                    DataFormat = NumberStyles.HexNumber;
                    break;

                case "bin":
                    DataFormat = NumberStyles.BinaryNumber;
                    break;

                default:
                    throw new Exception("Неподдерживаемый формат числа: " + format);
            }

            _selectedDataFormat = format;

            ViewData = ConvertNumberToString(Data, DataFormat);
        }

        public static ushort ConvertStringToNumber(string? value, NumberStyles format)
        {
            if (value == null || value == string.Empty)
            {
                return ushort.MinValue;
            }

            return ushort.Parse(value.Replace(" ", "").Replace("_", ""), format);
        }

        public static string ConvertNumberToString(ushort number, NumberStyles format)
        {
            switch (format)
            {
                case NumberStyles.BinaryNumber:
                    return GetFormattedBinaryNumber(number);

                case NumberStyles.Number:
                    return Convert.ToString(number, 10);

                case NumberStyles.HexNumber:
                    return Convert.ToString(number, 16).ToUpper();

                default:
                    throw new ArgumentException("Неподдерживаемый формат числа.");
            }            
        }

        private static string GetFormattedBinaryNumber(ushort number)
        {
            string binaryRepresentation = Convert.ToString(number, 2).PadLeft(16, '0');

            return string.Join(" ", new[]
            {
                binaryRepresentation.Substring(0, 4),
                binaryRepresentation.Substring(4, 4),
                binaryRepresentation.Substring(8, 4),
                binaryRepresentation.Substring(12, 4)
            });
        }
    }
}
