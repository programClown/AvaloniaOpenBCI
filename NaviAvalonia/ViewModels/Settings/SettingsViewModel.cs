﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using NaviAvalonia.Routing;
using ReactiveUI;

namespace NaviAvalonia.ViewModels.Settings;

public class SettingsViewModel : RoutableHostScreen<RoutableScreen>, IMainScreenViewModel
{
    private readonly IRouter _router;
    private RouteViewModel? _selectedTab;

    public SettingsViewModel(IRouter router)
    {
        _router = router;
        SettingTabs = new ObservableCollection<RouteViewModel> { };

        // Navigate on tab change
        this.WhenActivated(
            d =>
                this.WhenAnyValue(vm => vm.SelectedTab)
                    .WhereNotNull()
                    .Subscribe(
                        s => _router.Navigate(s.Path, new RouterNavigationOptions { IgnoreOnPartialMatch = true })
                    )
                    .DisposeWith(d)
        );
    }

    public ObservableCollection<RouteViewModel> SettingTabs { get; }

    public RouteViewModel? SelectedTab
    {
        get => _selectedTab;
        set => RaiseAndSetIfChanged(ref _selectedTab, value);
    }

    public ViewModelBase? TitleBarViewModel => null;

    /// <inheritdoc />
    public override async Task OnNavigating(NavigationArguments args, CancellationToken cancellationToken)
    {
        // Display tab change on navigate
        SelectedTab = SettingTabs.FirstOrDefault(t => t.Matches(args.Path));

        // Always show a tab, if there is none forward to the first
        if (SelectedTab == null)
            await _router.Navigate(SettingTabs.First().Path);
    }

    public void GoBack()
    {
        _router.Navigate("home");
    }
}
