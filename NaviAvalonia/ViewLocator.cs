﻿using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.ReactiveUI;
using NaviAvalonia.Extensions;
using ReactiveUI;

namespace NaviAvalonia;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        if (data == null)
            return new TextBlock { Text = "No data provided" };

        Type dataType = data.GetType();

        string name = dataType.FullName!.Split('`')[0].Replace("ViewModel", "View");
        Type? type = dataType.Assembly.GetType(name);

        // This isn't strictly required but it's super confusing (and happens to me all the time) if you implement IActivatableViewModel but forget to make your user control reactive.
        // When this happens your OnActivated never gets called and it's easy to miss.
        if (
            Debugger.IsAttached
            && data is IActivatableViewModel
            && type != null
            && !type.IsOfGenericType(typeof(ReactiveUserControl<>))
        )
            throw new Exception(
                $"The views of activatable view models should inherit ReactiveUserControl<T>, in this case ReactiveUserControl<{data.GetType().Name}>."
            );

        if (type != null)
            return (Control)Activator.CreateInstance(type)!;
        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ReactiveObject;
    }
}
