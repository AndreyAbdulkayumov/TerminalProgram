using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace TerminalProgram.Converters
{
    public class Binary_BoolToColorConverter : IValueConverter
    {
        public static readonly Binary_BoolToColorConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool IsChange)

                // TODO: для получения динамесчких ресурсов
                //if (Application.Current != null && Application.Current.TryFindResource(resourceKey, out var background))
                //{
                //    return background;
                //}
                return IsChange ? Brushes.LightPink : Brush.Parse("#FFDFDFDF");
            else
                throw new ArgumentException("Значение не является булевым.");
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
