using System.Globalization;

namespace ViewModels.Validation
{
    internal static class StringValue
    {
        public static bool IsValidNumber(string stringNumber, NumberStyles style, out byte number)
        {
            return byte.TryParse(stringNumber.Replace(" ", ""), style, CultureInfo.InvariantCulture, out number);
        }

        public static bool IsValidNumber(string stringNumber, NumberStyles style, out UInt16 number)
        {
            return UInt16.TryParse(stringNumber.Replace(" ", ""), style, CultureInfo.InvariantCulture, out number);
        }

        public static bool IsValidNumber(string stringNumber, NumberStyles style, out uint number)
        {
            return uint.TryParse(stringNumber.Replace(" ", ""), style, CultureInfo.InvariantCulture, out number);
        }
    }
}
