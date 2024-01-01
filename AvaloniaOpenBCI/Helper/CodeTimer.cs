using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace AvaloniaOpenBCI.Helper;

public class CodeTimer : IDisposable
{
    private static readonly Stack<CodeTimer> RunningTimers = new();

    private readonly string _name;
    private readonly Stopwatch _stopwatch;
    private bool _isDisposed;

    public CodeTimer(string postFix = "", [CallerMemberName] string callerName = "")
    {
        _name = $"{callerName}" + (string.IsNullOrEmpty(postFix) ? "" : $" ({postFix})");
        _stopwatch = Stopwatch.StartNew();

        // Set parent as the top of the stack
        if (RunningTimers.TryPeek(out CodeTimer? timer))
        {
            ParentTimer = timer;
            timer.SubTimers.Add(this);
        }

        // Add ourselves to the stack
        RunningTimers.Push(this);
    }

    private CodeTimer? ParentTimer { get; }
    private List<CodeTimer> SubTimers { get; } = new();

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _stopwatch.Stop();

        // Remove ourselves from stack
        if (RunningTimers.TryPop(out CodeTimer? timer))
        {
            if (timer != this)
            {
                throw new InvalidOperationException("Timer stack is corrupted");
            }
        }
        else
        {
            throw new InvalidOperationException("Timer stack is empty");
        }

        // If we're a root timer, output all results
        if (ParentTimer is null)
        {
            OutputDebug(GetResult());
            SubTimers.Clear();
        }

        _isDisposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Start a new timer and returns it if DEBUG is defined, otherwise returns an empty IDisposable
    /// </summary>
    /// <param name="postFix"></param>
    /// <param name="callerName"></param>
    /// <returns></returns>
    public static IDisposable StartDebug(
        string postFix = "",
        [CallerMemberName] string callerName = "")
    {
#if DEBUG
        return new CodeTimer(postFix, callerName);
#else
        return Disposable.Empty;
#endif
    }

    /// <summary>
    ///     Formats a TimeSpan into a string, Chooses the mose appropriate unit of time
    /// </summary>
    public static string FormatTime(TimeSpan duration)
    {
        if (duration.TotalSeconds < 1)
        {
            return $"{duration.TotalMilliseconds:0.00}ms";
        }

        if (duration.TotalMinutes < 1)
        {
            return $"{duration.TotalSeconds:0.00}s";
        }

        if (duration.TotalHours < 1)
        {
            return $"{duration.TotalMinutes:0.00}m";
        }

        return $"{duration.TotalHours:0.00}h";
    }

    private static void OutputDebug(string message)
    {
        Debug.WriteLine(message);
    }

    /// <summary>
    ///     Get results for this timer and all sub timers recursively
    /// </summary>
    private string GetResult()
    {
        var builder = new StringBuilder();

        builder.AppendLine($"{_name}: took {FormatTime(_stopwatch.Elapsed)}");

        foreach (CodeTimer timer in SubTimers)
        {
            // For each sub timer layer, add a `|-` prefix
            builder.AppendLine($"|- {timer.GetResult()}");
        }
        return builder.ToString();
    }
}