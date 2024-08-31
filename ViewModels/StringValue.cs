using MessageBox_Core;
using System.Globalization;

namespace ViewModels
{
    internal static class StringValue
    {
        public static Action<string, MessageType>? ShowMessageView;

        public static string CheckNumber(string? stringNumber, NumberStyles style, out Byte number)
        {
            if (stringNumber == null || stringNumber == String.Empty)
            {
                number = 0;
                return String.Empty;
            }

            if (Byte.TryParse(stringNumber, style, CultureInfo.InvariantCulture, out number) == false)
            {
                if (style == NumberStyles.HexNumber)
                {
                    ShowMessageView?.Invoke("Допустим ввод только чисел в шестнадцатеричной системе счисления.\n\nДиапазон чисел от 0x00 до 0xFF.", MessageType.Warning);
                }

                else
                {
                    ShowMessageView?.Invoke("Допустим ввод только чисел в десятичной системе счисления.\n\nДиапазон чисел от 0 до 255.", MessageType.Warning);
                }

                stringNumber = CorrectString(stringNumber, style, 2, 3);

                // Предполагаем что конвертация строки в число проходит успешно всегда,
                // т.к. ненужные символы были отсеяны в методе выше.
                UInt64.TryParse(stringNumber, style, CultureInfo.InvariantCulture, out UInt64 resultValue);

                if (resultValue > Byte.MaxValue)
                {
                    number = Byte.MaxValue;
                    stringNumber = Byte.MaxValue.ToString();
                }
            }

            return stringNumber;
        }

        public static string CheckNumber(string? stringNumber, NumberStyles style, out UInt16 number)
        {
            if (stringNumber == null || stringNumber == String.Empty)
            {
                number = 0;
                return String.Empty;
            }

            if (UInt16.TryParse(stringNumber, style, CultureInfo.InvariantCulture, out number) == false)
            {
                if (style == NumberStyles.HexNumber)
                {
                    ShowMessageView?.Invoke("Допустим ввод только чисел в шестнадцатеричной системе счисления.\n\nДиапазон чисел от 0x0000 до 0xFFFF.", MessageType.Warning);
                }

                else
                {
                    ShowMessageView?.Invoke("Допустим ввод только чисел в десятичной системе счисления.\n\nДиапазон чисел от 0 до 65535.", MessageType.Warning);
                }

                stringNumber = CorrectString(stringNumber, style, 4, 5);

                // Предполагаем что конвертация строки в число проходит успешно всегда,
                // т.к. ненужные символы были отсеяны в методе выше.
                UInt64.TryParse(stringNumber, style, CultureInfo.InvariantCulture, out UInt64 resultValue);

                if (resultValue > UInt16.MaxValue)
                {
                    number = UInt16.MaxValue;
                    stringNumber = UInt16.MaxValue.ToString();
                }
            }

            return stringNumber;
        }

        public static string CheckNumber(string? stringNumber, NumberStyles style, out UInt32 number)
        {
            if (stringNumber == null || stringNumber == String.Empty)
            {
                number = 0;
                return String.Empty;
            }

            if (UInt32.TryParse(stringNumber, style, CultureInfo.InvariantCulture, out number) == false)
            {
                if (style == NumberStyles.HexNumber)
                {
                    ShowMessageView?.Invoke("Допустим ввод только чисел в шестнадцатеричной системе счисления.\n\nДиапазон чисел от 0x00000000 до 0xFFFFFFFF.", MessageType.Warning);
                }

                else
                {
                    ShowMessageView?.Invoke("Допустим ввод только чисел в десятичной системе счисления.\n\nДиапазон чисел от 0 до 2^32.", MessageType.Warning);
                }

                stringNumber = CorrectString(stringNumber, style, 8, 10);

                // Предполагаем что конвертация строки в число проходит успешно всегда,
                // т.к. ненужные символы были отсеяны в методе выше.
                UInt64.TryParse(stringNumber, style, CultureInfo.InvariantCulture, out UInt64 resultValue);

                if (resultValue > UInt32.MaxValue)
                {
                    number = UInt32.MaxValue;
                    stringNumber = UInt32.MaxValue.ToString();
                }
            }

            return stringNumber;
        }

        private static string CorrectString(string stringNumber, NumberStyles style, int maxLength_Hex, int maxLength_Dec)
        {
            if (style == NumberStyles.HexNumber)
            {
                // Удаляем все нецифровые символы из введенного текста
                stringNumber = new string(stringNumber.Where(x => char.IsDigit(x) || "ABCDEFabcdef".Contains(x)).ToArray());

                // Ограничиваем длину строки до MaxLength_Hex символов (для шестнадцатеричного числа)
                stringNumber = stringNumber.Substring(0, Math.Min(stringNumber.Length, maxLength_Hex));
            }

            else
            {
                // Удаляем все нецифровые символы из введенного текста
                stringNumber = new string(stringNumber.Where(char.IsDigit).ToArray());

                // Ограничиваем длину строки до MaxLength_Dec символов (для десятичного числа)
                stringNumber = stringNumber.Substring(0, Math.Min(stringNumber.Length, maxLength_Dec));
            }

            if (stringNumber == null)
            {
                return String.Empty;
            }

            return stringNumber;
        }
    }
}
