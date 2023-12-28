using System;

namespace AvaloniaOpenBCI.ViewModels.Dialogs;

public class ExceptionViewModel : ViewModelBase
{
    public Exception? Exception { get; set; }

    public string? Message => Exception?.Message;

    public string? ExceptionType => Exception?.GetType().Name ?? "";
}