using System;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Avalonia.Threading;
using AvaloniaOpenBCI.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace AvaloniaOpenBCI.ViewModels.Base;

public partial class ViewModelBase : ObservableValidator, IRemovableListItem
{
    private WeakEventManager? _parentListRemoveRequestedEventManager;

    [PublicAPI]
    protected ViewModelState ViewModelState { get; private set; }

    public event EventHandler ParentListRemoveRequested
    {
        add
        {
            _parentListRemoveRequestedEventManager ??= new WeakEventManager();
            _parentListRemoveRequestedEventManager.AddEventHandler(value);
        }
        remove => _parentListRemoveRequestedEventManager?.RemoveEventHandler(value);
    }

    [RelayCommand]
    protected void RemoveFromParentList() =>
        _parentListRemoveRequestedEventManager?.RaiseEvent(
            this,
            EventArgs.Empty,
            nameof(ParentListRemoveRequested)
        );

    /// <summary>
    ///     Called when the view's LoadedEvent is fired.
    /// </summary>
    public virtual void OnLoaded()
    {
        if (!ViewModelState.HasFlag(ViewModelState.InitialLoaded))
        {
            ViewModelState |= ViewModelState.InitialLoaded;

            OnInitialLoaded();

            Dispatcher.UIThread.InvokeAsync(OnInitialLoadedAsync).SafeFireAndForget();
        }
    }

    /// <summary>
    ///     Called the first time the view's LoadedEvent is fired.
    ///     Sets the <see cref="ViewModelState.InitialLoaded" /> flag.
    /// </summary>
    protected virtual void OnInitialLoaded()
    {
    }

    /// <summary>
    ///     Called asynchronously when the view's LoadedEvent is fired.
    ///     Runs on the UI thread via Dispatcher.UIThread.InvokeAsync.
    ///     The view loading will not wait for this to complete.
    /// </summary>
    public virtual Task OnLoadedAsync() => Task.CompletedTask;

    /// <summary>
    ///     Called the first time the view's LoadedEvent is fired.
    ///     Sets the <see cref="ViewModelState.InitialLoaded" /> flag.
    /// </summary>
    protected virtual Task OnInitialLoadedAsync() => Task.CompletedTask;

    /// <summary>
    ///     Called when the view's UnloadedEvent is fired.
    /// </summary>
    public virtual void OnUnloaded()
    {
    }

    /// <summary>
    ///     Called asynchronously when the view's UnloadedEvent is fired.
    ///     Runs on the UI thread via Dispatcher.UIThread.InvokeAsync.
    ///     The view loading will not wait for this to complete.
    /// </summary>
    public virtual Task OnUnloadedAsync() => Task.CompletedTask;
}