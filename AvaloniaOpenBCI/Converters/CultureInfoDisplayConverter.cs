﻿using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AvaloniaOpenBCI.Converters;

public class CultureInfoDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not CultureInfo cultureInfo)
        {
            return null;
        }

        return cultureInfo.TextInfo.ToTitleCase(cultureInfo.NativeName);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
