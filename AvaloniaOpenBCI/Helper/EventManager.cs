using System;
using AvaloniaOpenBCI.Models.Progress;

namespace AvaloniaOpenBCI.Helper;

public class EventManager
{
    public static EventManager Instance { get; } = new();

    private EventManager() { }

    public event EventHandler<int>? GlobalProcessChanged;

    public event EventHandler? TeachingTooltipNeeded;

    public event EventHandler? ScrollToBottomRequested;

    public event EventHandler<ProgressItem>? ProgressChanged;

    public event EventHandler? ToggleProgressFlyout;

    public virtual void OnGlobalProcessChanged(int e) => GlobalProcessChanged?.Invoke(this, e);

    public virtual void OnTeachingTooltipNeeded() => TeachingTooltipNeeded?.Invoke(this, EventArgs.Empty);

    public virtual void OnScrollToBottomRequested() => ScrollToBottomRequested?.Invoke(this, EventArgs.Empty);

    public virtual void OnProgressChanged(ProgressItem e) => ProgressChanged?.Invoke(this, e);

    public virtual void OnToggleProgressFlyout() => ToggleProgressFlyout?.Invoke(this, EventArgs.Empty);
}
