﻿using System;

namespace NaviAvalonia.Routing;

/// <summary>
///     Represents a view model to which routing with parameters can take place and which in turn can host another view
///     model.
/// </summary>
/// <typeparam name="TScreen">The type of view model the screen can host.</typeparam>
/// <typeparam name="TParam">The type of parameters the screen expects. It must have a parameterless constructor.</typeparam>
public abstract class RoutableHostScreen<TScreen, TParam> : RoutableScreen<TParam>, IRoutableHostScreen
    where TScreen : RoutableScreen
    where TParam : new()
{
    private bool _recycleScreen = true;
    private TScreen? _screen;

    /// <summary>
    ///     Gets the currently active child screen.
    /// </summary>
    public TScreen? Screen
    {
        get => _screen;
        private set => RaiseAndSetIfChanged(ref _screen, value);
    }

    /// <inheritdoc />
    public bool RecycleScreen
    {
        get => _recycleScreen;
        protected set => RaiseAndSetIfChanged(ref _recycleScreen, value);
    }

    IRoutableScreen? IRoutableHostScreen.InternalScreen => Screen;

    void IRoutableHostScreen.InternalChangeScreen(IRoutableScreen? screen)
    {
        if (screen == null)
        {
            Screen = null;
            return;
        }

        if (screen is not TScreen typedScreen)
            throw new Exception(
                $"Screen cannot be hosted, {screen.GetType().Name} is not assignable to {typeof(TScreen).Name}."
            );
        Screen = typedScreen;
    }
}
