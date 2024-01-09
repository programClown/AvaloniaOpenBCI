﻿using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AvaloniaOpenBCI.Converters;

public class IndexPlusOneConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int i)
        {
            return i + 1;
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int i)
        {
            return i - 1;
        }

        return value;
    }
}
