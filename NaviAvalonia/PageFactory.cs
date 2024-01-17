﻿using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;

namespace NaviAvalonia;

public class PageFactory : INavigationPageFactory
{
    private readonly ViewLocator _viewLocator = new();

    /// <inheritdoc />
    public Control? GetPage(Type srcType)
    {
        return null;
    }

    /// <inheritdoc />
    public Control GetPageFromObject(object target)
    {
        Control control = _viewLocator.Build(target);
        control.DataContext = target;
        return control;
    }
}
