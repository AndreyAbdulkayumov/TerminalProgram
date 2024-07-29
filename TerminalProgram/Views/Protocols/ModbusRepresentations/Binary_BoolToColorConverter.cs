using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace TerminalProgram.Views.Protocols.ModbusRepresentations
{
    public class Binary_BoolToColorConverter : IValueConverter
    {
        public static readonly Binary_BoolToColorConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool IsChange)
                return IsChange ? Brushes.LightPink : Brushes.Silver;
            else
                throw new ArgumentException("Значение не является булевым.");
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
