using System;
using System.Diagnostics.CodeAnalysis;
using AvaloniaOpenBCI.Models.Configs;
using AvaloniaOpenBCI.ViewModels.Base;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;

namespace AvaloniaOpenBCI.Services;

public interface INavigationService<[SuppressMessage("ReSharper", "UnusedTypeParameter")] T>
{
    event EventHandler<TypedNavigationEventArgs>? TypedNavigation;

    void SetFrame(Frame frame);

    void NavigateTo<TViewModel>(NavigationTransitionInfo? transitionInfo = null, object? param = null)
        where TViewModel : ViewModelBase;

    void NavigateTo(Type viewModelType, NavigationTransitionInfo? transitionInfo = null, object? param = null);

    void NavigateTo(ViewModelBase viewModel, NavigationTransitionInfo? transitionInfo = null, object? param = null);
}
