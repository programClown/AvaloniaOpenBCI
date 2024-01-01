﻿using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using System;

namespace AvaloniaOpenBCI.ViewModels.Base;

public partial class ContentDialogProgressViewModelBase : ConsoleProgressViewModel
{
    [ObservableProperty]
    private bool _hideCloseButton;

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

    public virtual void OnClodeButtonClick()
    {
        CloseButtonClick?.Invoke(this, ContentDialogResult.None);
    }
}