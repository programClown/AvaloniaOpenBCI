﻿using System;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Threading;

namespace NaviAvalonia.Helper;

/// <summary>
///     Static UI helpers.
/// </summary>
public static class UI
{
    private static readonly BehaviorSubject<bool> MicaEnabledSubject = new(false);

    public static EventLoopScheduler BackgroundScheduler = new(ts => new Thread(ts));

    static UI()
    {
        CurrentKeyBindingsEnabled = InputElement
            .GotFocusEvent.Raised.Select(e => e.Item2.Source is not TextBox)
            .StartWith(true);

        CurrentKeyBindingsEnabled.Subscribe(b => KeyBindingsEnabled = b);
        MicaEnabled = MicaEnabledSubject.AsObservable();
    }

    /// <summary>
    ///     Gets the current IoC locator.
    /// </summary>
    public static IContainer Locator { get; set; } = null!;

    /// <summary>
    ///     Gets the clipboard.
    /// </summary>
    public static IClipboard Clipboard { get; set; } = null!;

    /// <summary>
    ///     Gets an observable boolean indicating whether hotkeys are to be disabled.
    /// </summary>
    public static IObservable<bool> CurrentKeyBindingsEnabled { get; }

    /// <summary>
    ///     Gets a boolean indicating whether hotkeys are to be disabled.
    /// </summary>
    public static bool KeyBindingsEnabled { get; private set; }

    /// <summary>
    ///     Gets a boolean indicating whether the Mica effect should be enabled.
    /// </summary>
    public static IObservable<bool> MicaEnabled { get; }

    /// <summary>
    ///     Changes whether Mica should be enabled.
    /// </summary>
    /// <param name="enabled"></param>
    public static void SetMicaEnabled(bool enabled)
    {
        if (MicaEnabledSubject.Value != enabled)
            Dispatcher.UIThread.Invoke(() => MicaEnabledSubject.OnNext(enabled));
    }
}
