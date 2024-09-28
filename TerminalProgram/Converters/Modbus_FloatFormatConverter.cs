using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace TerminalProgram.Converters
{
    public class Modbus_FloatFormatConverter : IValueConverter
    {
        public static readonly Modbus_FloatFormatConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.Equals(parameter);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
            {
                return parameter;
            }

            // В противном случае возвращаем BindingOperations.DoNothing,
            // чтобы указать на отсутствие изменений
            return BindingOperations.DoNothing;
        }
    }
}
