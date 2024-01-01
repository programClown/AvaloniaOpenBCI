using AvaloniaOpenBCI.Processes;
using System;

namespace AvaloniaOpenBCI.Exceptions;

public class ProcessException : Exception
{
    public ProcessException(string message) : base(message)
    {
    }

    public ProcessException(ProcessResult processResult)
        : base(
            $"Process {processResult.ProcessName} exited with code {processResult.ExitCode}. {{StdOut = {processResult.StandardOutput}, StdErr = {processResult.StandardError}}}"
        )
    {
        ProcessResult = processResult;
    }

    public ProcessResult? ProcessResult { get; }
}