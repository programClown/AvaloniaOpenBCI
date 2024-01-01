using AvaloniaOpenBCI.Exceptions;
using System;

namespace AvaloniaOpenBCI.Processes;

public readonly record struct ProcessResult
{
    public required int ExitCode { get; init; }
    public string? StandardOutput { get; init; }
    public string? StandardError { get; init; }

    public string? ProcessName { get; init; }

    public TimeSpan Elapsed { get; init; }

    public bool IsSuccessExitCode
    {
        get => ExitCode == 0;
    }

    public void EnsureSuccessExitCode()
    {
        if (!IsSuccessExitCode)
        {
            throw new ProcessException(this);
        }
    }
}