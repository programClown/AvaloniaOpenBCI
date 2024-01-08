using System;
using AvaloniaOpenBCI.Models.Configs;
using AvaloniaOpenBCI.ViewModels.Base;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;

namespace AvaloniaOpenBCI.Services;

public class NavigationService<T> : INavigationService<T>
{
    private Frame? _frame;
    public event EventHandler<TypedNavigationEventArgs>? TypedNavigation;

    public void SetFrame(Frame frame)
    {
        _frame = frame;
    }

    public void NavigateTo<TViewModel>(NavigationTransitionInfo? transitionInfo = null, object? param = null)
        where TViewModel : ViewModelBase
    {
        if (_frame is null)
        {
            throw new InvalidOperationException("SetFrame was not called before NavigateTo.");
        }

        _frame.NavigateToType(
            typeof(TViewModel),
            param,
            new FrameNavigationOptions
            {
                IsNavigationStackEnabled = true,
                TransitionInfoOverride = transitionInfo ?? new SuppressNavigationTransitionInfo()
            }
        );

        TypedNavigation?.Invoke(this, new TypedNavigationEventArgs { ViewModelType = typeof(TViewModel) });
    }

    public void NavigateTo(Type viewModelType, NavigationTransitionInfo? transitionInfo = null, object? param = null)
    {
        if (!viewModelType.IsAssignableTo(typeof(ViewModelBase)))
        {
            // ReSharper disable once LocalizableElement
            throw new ArgumentException("Type must be a ViewModelBase.", nameof(viewModelType));
        }

        if (_frame is null)
        {
            throw new InvalidOperationException("SetFrame was not called before NavigateTo.");
        }

        _frame.NavigateToType(
            viewModelType,
            param,
            new FrameNavigationOptions
            {
                IsNavigationStackEnabled = true,
                TransitionInfoOverride = transitionInfo ?? new SuppressNavigationTransitionInfo()
            }
        );

        TypedNavigation?.Invoke(this, new TypedNavigationEventArgs { ViewModelType = viewModelType });
    }

    public void NavigateTo(
        ViewModelBase viewModel,
        NavigationTransitionInfo? transitionInfo = null,
        object? param = null
    )
    {
        if (_frame is null)
        {
            throw new InvalidOperationException("SetFrame was not called before NavigateTo.");
        }

        _frame.NavigateFromObject(
            viewModel,
            new FrameNavigationOptions
            {
                IsNavigationStackEnabled = true,
                TransitionInfoOverride = transitionInfo ?? new SuppressNavigationTransitionInfo()
            }
        );

        TypedNavigation?.Invoke(
            this,
            new TypedNavigationEventArgs { ViewModelType = viewModel.GetType(), ViewModel = viewModel }
        );
    }
}
