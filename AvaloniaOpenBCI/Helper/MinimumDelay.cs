using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AvaloniaOpenBCI.Helper;

/// <summary>
/// Enforces a minimum delay if the function returns too quickly.
/// Waits during async Dispose.
/// </summary>
public class MinimumDelay : IAsyncDisposable
{
    private readonly Stopwatch _stopwatch = new();
    private readonly TimeSpan _delay;

    /// <summary>
    /// Minimum random delay in milliseconds.
    /// </summary>
    public MinimumDelay(int randMin, int randMax)
    {
        _stopwatch.Start();
        Random rand = new Random();
        _delay = TimeSpan.FromMilliseconds(rand.Next(randMin, randMax));
    }

    /// <summary>
    /// Minimum fixed delay in milliseconds.
    /// </summary>
    public MinimumDelay(int delayMilliseconds)
    {
        _stopwatch.Start();
        _delay = TimeSpan.FromMilliseconds(delayMilliseconds);
    }

    public async ValueTask DisposeAsync()
    {
        _stopwatch.Stop();
        var elapsed = _stopwatch.Elapsed;
        if (elapsed < _delay)
        {
            await Task.Delay(_delay - elapsed);
        }
        GC.SuppressFinalize(this);
    }
}
