using System;

namespace AvaloniaOpenBCI.Models.Configs;

public class TypedNavigationEventArgs : EventArgs
{
    public required Type ViewModelType { get; init; }

    public object? ViewModel { get; init; }
}
