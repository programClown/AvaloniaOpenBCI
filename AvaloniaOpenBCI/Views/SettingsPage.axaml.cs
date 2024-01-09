using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using AvaloniaOpenBCI.Animations;
using AvaloniaOpenBCI.Controls;
using AvaloniaOpenBCI.Models.Configs;
using AvaloniaOpenBCI.Services;
using AvaloniaOpenBCI.ViewModels;
using AvaloniaOpenBCI.ViewModels.Base;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;

namespace AvaloniaOpenBCI.Views;

public partial class SettingsPage : UserControlBase
{
    private readonly INavigationService<SettingsViewModel> _settingsNavigationService;

    private bool _hasLoaded;

    private SettingsViewModel ViewModel => (SettingsViewModel)DataContext;

    public SettingsPage(INavigationService<SettingsViewModel> settingsNavigationService)
    {
        _settingsNavigationService = settingsNavigationService;

        InitializeComponent();

        _settingsNavigationService.SetFrame(FrameView);
        _settingsNavigationService.TypedNavigation += NavigationServiceOnTypedNavigation;
        FrameView.Navigated += FrameViewOnNavigated;
        BreadcrumbBar.ItemClicked += BreadcrumbBarOnItemClicked;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (!_hasLoaded)
        {
            // Initial load, navigate to first page
            Dispatcher.UIThread.Post(
                () =>
                    _settingsNavigationService.NavigateTo(ViewModel.SubPages[0], new SuppressNavigationTransitionInfo())
            );

            _hasLoaded = true;
        }
    }

    private void NavigationServiceOnTypedNavigation(object? sender, TypedNavigationEventArgs e)
    {
        ViewModel.CurrentPage = ViewModel.SubPages.FirstOrDefault(x => x.GetType() == e.ViewModelType);
    }

    private void FrameViewOnNavigated(object sender, NavigationEventArgs e)
    {
        if (e.Content is not PageViewModelBase vm)
        {
            return;
        }

        ViewModel.CurrentPage = vm;
    }

    private void BreadcrumbBarOnItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        // Skip if already on same page
        if (args.Item is not PageViewModelBase viewModel || viewModel == ViewModel.CurrentPage)
        {
            return;
        }

        _settingsNavigationService.NavigateTo(viewModel, BetterSlideNavigationTransition.PageSlideFromLeft);
    }
}
