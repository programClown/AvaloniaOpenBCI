using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AvaloniaOpenBCI.Converters;

public class FitSquarelyWithinAspectRatioConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Rect bounds = value is Rect rect ? rect : default;
        return Math.Min(bounds.Width, bounds.Height);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}