using ReactiveUI;
using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace ViewModels.ModbusClient.WriteFields
{
    public abstract class ModbusDataFormatter : ReactiveObject
    {
        public const string DataFormatName_dec = "dec";
        public const string DataFormatName_hex = "hex";
        public const string DataFormatName_bin = "bin";

        public abstract string? ViewData { get; set; }

        public abstract void SetDataFormat(string format);

        protected ushort ConvertStringToNumber(string? value, NumberStyles format)
        {
            if (value == null || value == string.Empty)
            {
                return ushort.MinValue;
            }

            return ushort.Parse(value.Replace(" ", "").Replace("_", ""), format);
        }

        protected string ConvertNumberToString(ushort number, NumberStyles format)
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

        private string GetFormattedBinaryNumber(ushort number)
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
