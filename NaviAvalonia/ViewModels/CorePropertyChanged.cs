﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace NaviAvalonia.ViewModels;

/// <summary>
///     Represents a basic bindable class which notifies when a property value changes.
/// </summary>
public abstract class CorePropertyChanged : INotifyPropertyChanged
{
    /// <summary>
    ///     Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    #region Methods

    /// <summary>
    ///     Checks if the property already matches the desired value or needs to be updated.
    /// </summary>
    /// <typeparam name="T">Type of the property.</typeparam>
    /// <param name="storage">Reference to the backing-filed.</param>
    /// <param name="value">Value to apply.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool RequiresUpdate<T>(ref T storage, T value)
    {
        return !EqualityComparer<T>.Default.Equals(storage, value);
    }

    /// <summary>
    ///     Checks if the property already matches the desired value and updates it if not.
    /// </summary>
    /// <typeparam name="T">Type of the property.</typeparam>
    /// <param name="storage">Reference to the backing-filed.</param>
    /// <param name="value">Value to apply.</param>
    /// <param name="propertyName">
    ///     Name of the property used to notify listeners. This value is optional
    ///     and can be provided automatically when invoked from compilers that support <see cref="CallerMemberNameAttribute" />
    ///     .
    /// </param>
    /// <returns><c>true</c> if the value was changed, <c>false</c> if the existing value matched the desired value.</returns>
    [NotifyPropertyChangedInvocator]
    protected bool SetAndNotify<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (!RequiresUpdate(ref storage, value))
            return false;

        storage = value;
        // ReSharper disable once ExplicitCallerInfoArgument
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    ///     Triggers the <see cref="PropertyChanged" />-event when a a property value has changed.
    /// </summary>
    /// <param name="propertyName">
    ///     Name of the property used to notify listeners. This value is optional
    ///     and can be provided automatically when invoked from compilers that support <see cref="CallerMemberNameAttribute" />
    ///     .
    /// </param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}
