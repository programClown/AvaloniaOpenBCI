using Avalonia.Data.Converters;
using SharpCompress;

namespace AvaloniaOpenBCI.Converters;

public static class StringFormatConverters
{
    private static StringFormatValueConverter? _decimalConverter;
    public static StringFormatValueConverter Decimal =>
        _decimalConverter ??= new StringFormatValueConverter("{0:D}", null);

    private static readonly Lazy<IValueConverter> MemoryBytesConverterLazy =
        new(() => new CustomStringFormatConverter<MemoryBytesFormatter>("{0:M}"));

    public static IValueConverter MemoryBytes => MemoryBytesConverterLazy.Value;
}
