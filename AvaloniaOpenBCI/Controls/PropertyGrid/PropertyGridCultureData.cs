using System;
using System.Globalization;
using PropertyModels.Localilzation;

namespace AvaloniaOpenBCI.Controls;

internal class PropertyGridCultureData : ICultureData
{
    /// <inheritdoc />
    public bool Reload() => false;

    /// <inheritdoc />
    public CultureInfo Culture => CultureInfo.CurrentCulture;

    /// <inheritdoc />
    public Uri Path => new("");

    /// <inheritdoc />
    public string this[string key] => key;

    /// <inheritdoc />
    public bool IsLoaded => true;
}
