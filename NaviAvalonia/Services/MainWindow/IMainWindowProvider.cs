﻿using System;

namespace NaviAvalonia.Services.MainWindow;

/// <summary>
///     Represents a class that provides the main window, so that <see cref="IMainWindowService" /> can control the state
///     of
///     the main window.
/// </summary>
public interface IMainWindowProvider
{
    /// <summary>
    ///     Gets a boolean indicating whether the main window is currently open
    /// </summary>
    bool IsMainWindowOpen { get; }

    /// <summary>
    ///     Gets a boolean indicating whether the main window is currently focused
    /// </summary>
    bool IsMainWindowFocused { get; }

    /// <summary>
    ///     Opens the main window
    /// </summary>
    void OpenMainWindow();

    /// <summary>
    ///     Closes the main window
    /// </summary>
    void CloseMainWindow();

    /// <summary>
    ///     Occurs when the main window has been opened
    /// </summary>
    public event EventHandler? MainWindowOpened;

    /// <summary>
    ///     Occurs when the main window has been closed
    /// </summary>
    public event EventHandler? MainWindowClosed;

    /// <summary>
    ///     Occurs when the main window has been focused
    /// </summary>
    public event EventHandler? MainWindowFocused;

    /// <summary>
    ///     Occurs when the main window has been unfocused
    /// </summary>
    public event EventHandler? MainWindowUnfocused;
}
