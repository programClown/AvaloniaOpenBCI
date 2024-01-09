using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvaloniaOpenBCI.Attributes;
using AvaloniaOpenBCI.ViewModels.Base;
using FluentAvalonia.UI.Controls;

namespace AvaloniaOpenBCI;

public class ViewLocator : IDataTemplate, INavigationPageFactory
{
    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }

        var name = data.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type != null)
        {
            var control = (Control)Activator.CreateInstance(type)!;
            control.DataContext = data;
            return control;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }

    private Control GetView(Type viewType)
    {
        if (App.Services.GetService(viewType) is Control view)
        {
            return view;
        }

        return new TextBlock { Text = "View Not Found: " + viewType.FullName };
    }

    /// <inheritdoc />
    public Control GetPage(Type srcType)
    {
        if (Attribute.GetCustomAttribute(srcType, typeof(ViewAttribute)) is not ViewAttribute viewAttr)
        {
            throw new InvalidOperationException("View not found for " + srcType.FullName);
        }

        // Get new view
        var view = GetView(viewAttr.ViewType);
        view.DataContext ??= App.Services.GetService(srcType);

        return view;
    }

    public Control GetPageFromObject(object target)
    {
        if (Attribute.GetCustomAttribute(target.GetType(), typeof(ViewAttribute)) is not ViewAttribute viewAttr)
        {
            throw new InvalidOperationException("View not found for " + target.GetType().FullName);
        }

        var viewType = viewAttr.ViewType;
        var view = GetView(viewType);
        view.DataContext ??= target;
        return view;
    }
}
