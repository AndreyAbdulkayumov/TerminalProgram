using ReactiveUI;
using System.Collections.ObjectModel;
using System.Globalization;

namespace ViewModels.ModbusClient
{
    public class ModbusClient_WriteData_VM : ReactiveObject
    {
        private ushort _address = 0;

        public ushort Address
        {
            get => _address;
            set
            {
                _address = value;
            }
        }

        private string? _viewAddress;

        public string? ViewAddress
        {
            get => _viewAddress;
            set
            {
                this.RaiseAndSetIfChanged(ref _viewAddress, value);
            }
        }

        private string? _addressNumberFormat;

        public string? AddressNumberFormat
        {
            get => _addressNumberFormat;
            set => this.RaiseAndSetIfChanged(ref _addressNumberFormat, value);
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


        public ModbusClient_WriteData_VM(ushort Address, ushort Data, string DataFormat)
        {
            SelectedDataFormat = DataFormat;

            this.Address = Address;
            this.Data = Data;

            ViewData = ConvertNumberToString(Data, this.DataFormat);
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
            if (value == null)
            {
                return ushort.MinValue;
            }

            return ushort.Parse(value, format);
        }

        public static string ConvertNumberToString(ushort number, NumberStyles format)
        {
            int baseFormat;

            switch (format)
            {
                case NumberStyles.BinaryNumber:
                    baseFormat = 2;
                    break;

                case NumberStyles.Number:
                    baseFormat = 10;
                    break;

                case NumberStyles.HexNumber:
                    baseFormat = 16;
                    break;

                default:
                    throw new ArgumentException("Неподдерживаемый формат числа.");
            }

            return Convert.ToString(number, baseFormat).ToUpper();
        }
    }
}
