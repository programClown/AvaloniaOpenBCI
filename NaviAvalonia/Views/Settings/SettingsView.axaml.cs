using System;
using System.Reactive.Disposables;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using NaviAvalonia.ViewModels;
using NaviAvalonia.ViewModels.Settings;
using ReactiveUI;

namespace NaviAvalonia.Views.Settings;

public partial class SettingsView : ReactiveUserControl<SettingsViewModel>
{
    public SettingsView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            ViewModel.WhenAnyValue(vm => vm.Screen).WhereNotNull().Subscribe(Navigate).DisposeWith(d);
        });
    }

    private void Navigate(ViewModelBase viewModel)
    {
        Dispatcher.UIThread.Invoke(() => TabFrame.NavigateFromObject(viewModel));
    }

    private void NavigationView_OnBackRequested(object? sender, NavigationViewBackRequestedEventArgs e)
    {
        ViewModel?.GoBack();
    }
}
