﻿using Avalonia.Threading;
using AvaloniaOpenBCI.Controls;
using FluentAvalonia.UI.Controls;
using System;

namespace AvaloniaOpenBCI.ViewModels.Base;

public class ContentDialogViewModelBase : ViewModelBase
{
    public virtual string? Title { get; set; }

    //Events for button clicks
    public event EventHandler<ContentDialogResult>? PrimaryButtonClick;
    public event EventHandler<ContentDialogResult>? SecondaryButtonClick;
    public event EventHandler<ContentDialogResult>? CloseButtonClick;

    public virtual void OnPrimaryButtonClick()
    {
        PrimaryButtonClick?.Invoke(this, ContentDialogResult.Primary);
    }

    public virtual void OnSecondaryButtonClick()
    {
        SecondaryButtonClick?.Invoke(this, ContentDialogResult.Secondary);
    }

    public virtual void OnCloseButtonClick()
    {
        CloseButtonClick?.Invoke(this, ContentDialogResult.None);
    }

    /// <summary>
    ///     Return a <see cref="BetterContentDialog" /> that uses this view model as its content
    /// </summary>
    public virtual BetterContentDialog GetDialog()
    {
        Dispatcher.UIThread.VerifyAccess();

        var dialog = new BetterContentDialog
        {
            Title = Title,
            Content = this
        };

        return dialog;
    }
}