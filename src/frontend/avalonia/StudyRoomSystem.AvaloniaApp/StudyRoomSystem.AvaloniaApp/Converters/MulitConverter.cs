using System;
using System.Globalization;
using System.Numerics;
using Avalonia.Data.Converters;

namespace StudyRoomSystem.AvaloniaApp.Converters;

public sealed partial class MulitConverter : IValueConverter
{
    public static IValueConverter Instance { get; } = new MulitConverter();

    private MulitConverter() { }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (double.TryParse(value?.ToString(), out var o))
        {
            if (double.TryParse(parameter?.ToString(), out var p))
            {
                return o * p;
            }

            return o;
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}