using System;

namespace AvaloniaOpenBCI.Exceptions;

public class AppException : ApplicationException
{
    public override string Message { get; }
    public string? Details { get; init; }

    public AppException(string message)
    {
        Message = message;
    }

    public AppException(string message, string details)
    {
        Message = message;
        Details = details;
    }
}
