using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace View_WPF.Views
{
    internal static class StringValue
    {
        public static string CheckNumber(string StringNumber, NumberStyles Style, out Byte Number)
        {
            if (StringNumber == String.Empty)
            {
                Number = 0;
                return StringNumber;
            }

            if (Byte.TryParse(StringNumber, Style, CultureInfo.InvariantCulture, out Number) == false)
            {
                if (Style == NumberStyles.HexNumber)
                {
                    MessageView.Show("Допустим только ввод чисел в шестнадцатеричной системе счисления.\n\nДиапазон чисел от 0x00 до 0xFF.", MessageType.Warning);
                }

                else
                {
                    MessageView.Show("Допустим только ввод чисел в десятичной системе счисления.\n\nДиапазон чисел от 0 до 255.", MessageType.Warning);
                }

                StringNumber = CorrectString(StringNumber, Style, 2, 3);
            }

            return StringNumber;
        }

        public static string CheckNumber(string StringNumber, NumberStyles Style, out UInt16 Number)
        {
            if (StringNumber == String.Empty)
            {
                Number = 0;
                return StringNumber;
            }

            if (UInt16.TryParse(StringNumber, Style, CultureInfo.InvariantCulture, out Number) == false)
            {
                if (Style == NumberStyles.HexNumber)
                {
                    MessageView.Show("Допустим только ввод чисел в шестнадцатеричной системе счисления.\n\nДиапазон чисел от 0x0000 до 0xFFFF.", MessageType.Warning);
                }

                else
                {
                    MessageView.Show("Допустим только ввод чисел в десятичной системе счисления.\n\nДиапазон чисел от 0 до 65535.", MessageType.Warning);
                }

                StringNumber = CorrectString(StringNumber, Style, 4, 5);
            }

            return StringNumber;
        }

        public static string CheckNumber(string StringNumber, NumberStyles Style, out UInt32 Number)
        {
            if (StringNumber == String.Empty)
            {
                Number = 0;
                return StringNumber;
            }

            if (UInt32.TryParse(StringNumber, Style, CultureInfo.InvariantCulture, out Number) == false)
            {
                if (Style == NumberStyles.HexNumber)
                {
                    MessageView.Show("Допустим только ввод чисел в шестнадцатеричной системе счисления.\n\nДиапазон чисел от 0x00000000 до 0xFFFFFFFF.", MessageType.Warning);
                }

                else
                {
                    MessageView.Show("Допустим только ввод чисел в десятичной системе счисления.\n\nДиапазон чисел от 0 до 2^32.", MessageType.Warning);
                }

                StringNumber = CorrectString(StringNumber, Style, 4, 5);
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
