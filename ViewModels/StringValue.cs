using MessageBox_Core;
using System.Globalization;

namespace ViewModels
{
    internal static class StringValue
    {
        public static Action<string, MessageType>? ShowMessageView;

        public static string CheckNumber(string? StringNumber, NumberStyles Style, out Byte Number)
        {
            if (StringNumber == null || StringNumber == String.Empty)
            {
                Number = 0;
                return String.Empty;
            }

            if (Byte.TryParse(StringNumber, Style, CultureInfo.InvariantCulture, out Number) == false)
            {
                if (Style == NumberStyles.HexNumber)
                {
                    ShowMessageView?.Invoke("Допустим ввод только чисел в шестнадцатеричной системе счисления.\n\nДиапазон чисел от 0x00 до 0xFF.", MessageType.Warning);
                }

                else
                {
                    ShowMessageView?.Invoke("Допустим ввод только чисел в десятичной системе счисления.\n\nДиапазон чисел от 0 до 255.", MessageType.Warning);
                }

                StringNumber = CorrectString(StringNumber, Style, 2, 3);

                // Предполагаем что конвертация строки в число проходит успешно всегда,
                // т.к. ненужные символы были отсеяны в методе выше.
                UInt64.TryParse(StringNumber, Style, CultureInfo.InvariantCulture, out UInt64 ResultValue);

                if (ResultValue > Byte.MaxValue)
                {
                    Number = Byte.MaxValue;
                    StringNumber = Byte.MaxValue.ToString();
                }
            }

            return StringNumber;
        }

        public static string CheckNumber(string? StringNumber, NumberStyles Style, out UInt16 Number)
        {
            if (StringNumber == null || StringNumber == String.Empty)
            {
                Number = 0;
                return String.Empty;
            }

            if (UInt16.TryParse(StringNumber, Style, CultureInfo.InvariantCulture, out Number) == false)
            {
                if (Style == NumberStyles.HexNumber)
                {
                    ShowMessageView?.Invoke("Допустим ввод только чисел в шестнадцатеричной системе счисления.\n\nДиапазон чисел от 0x0000 до 0xFFFF.", MessageType.Warning);
                }

                else
                {
                    ShowMessageView?.Invoke("Допустим ввод только чисел в десятичной системе счисления.\n\nДиапазон чисел от 0 до 65535.", MessageType.Warning);
                }

                StringNumber = CorrectString(StringNumber, Style, 4, 5);

                // Предполагаем что конвертация строки в число проходит успешно всегда,
                // т.к. ненужные символы были отсеяны в методе выше.
                UInt64.TryParse(StringNumber, Style, CultureInfo.InvariantCulture, out UInt64 ResultValue);

                if (ResultValue > UInt16.MaxValue)
                {
                    Number = UInt16.MaxValue;
                    StringNumber = UInt16.MaxValue.ToString();
                }
            }

            return StringNumber;
        }

        public static string CheckNumber(string? StringNumber, NumberStyles Style, out UInt32 Number)
        {
            if (StringNumber == null || StringNumber == String.Empty)
            {
                Number = 0;
                return String.Empty;
            }

            if (UInt32.TryParse(StringNumber, Style, CultureInfo.InvariantCulture, out Number) == false)
            {
                if (Style == NumberStyles.HexNumber)
                {
                    ShowMessageView?.Invoke("Допустим ввод только чисел в шестнадцатеричной системе счисления.\n\nДиапазон чисел от 0x00000000 до 0xFFFFFFFF.", MessageType.Warning);
                }

                else
                {
                    ShowMessageView?.Invoke("Допустим ввод только чисел в десятичной системе счисления.\n\nДиапазон чисел от 0 до 2^32.", MessageType.Warning);
                }

                StringNumber = CorrectString(StringNumber, Style, 8, 10);

                // Предполагаем что конвертация строки в число проходит успешно всегда,
                // т.к. ненужные символы были отсеяны в методе выше.
                UInt64.TryParse(StringNumber, Style, CultureInfo.InvariantCulture, out UInt64 ResultValue);

                if (ResultValue > UInt32.MaxValue)
                {
                    Number = UInt32.MaxValue;
                    StringNumber = UInt32.MaxValue.ToString();
                }
            }

            return StringNumber;
        }

        private static string CorrectString(string StringNumber, NumberStyles Style, int MaxLength_Hex, int MaxLength_Dec)
        {
            if (Style == NumberStyles.HexNumber)
            {
                // Удаляем все нецифровые символы из введенного текста
                StringNumber = new string(StringNumber.Where(x => char.IsDigit(x) || "ABCDEFabcdef".Contains(x)).ToArray());

                // Ограничиваем длину строки до MaxLength_Hex символов (для шестнадцатеричного числа)
                StringNumber = StringNumber.Substring(0, Math.Min(StringNumber.Length, MaxLength_Hex));
            }

            else
            {
                // Удаляем все нецифровые символы из введенного текста
                StringNumber = new string(StringNumber.Where(char.IsDigit).ToArray());

                // Ограничиваем длину строки до MaxLength_Dec символов (для десятичного числа)
                StringNumber = StringNumber.Substring(0, Math.Min(StringNumber.Length, MaxLength_Dec));
            }

            if (StringNumber == null)
            {
                return String.Empty;
            }

            return StringNumber;
        }
    }
}
