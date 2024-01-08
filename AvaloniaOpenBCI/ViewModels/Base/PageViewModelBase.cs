﻿using FluentAvalonia.UI.Controls;

namespace AvaloniaOpenBCI.ViewModels.Base;

/// <summary>
/// An abstract class for enabling page navigation
/// </summary>
public abstract class PageViewModelBase : ViewModelBase
{
    /// <summary>
    /// Gets if the user can navigate to rhe next page
    /// </summary>
    public virtual bool CanNavigateNext { get; protected set; }

    /// <summary>
    /// Gets if the user can navigate to rhe previous page
    /// </summary>
    public virtual bool CanNavigatePrevious { get; protected set; }

    public abstract string Title { get; }

    public abstract IconSource IconSource { get; }
}
