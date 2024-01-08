using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using AsyncImageLoader;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using Avalonia.Threading;
using AvaloniaOpenBCI.Animations;
using AvaloniaOpenBCI.Controls;
using AvaloniaOpenBCI.Helper;
using AvaloniaOpenBCI.Models.Configs;
using AvaloniaOpenBCI.Services;
using AvaloniaOpenBCI.ViewModels;
using AvaloniaOpenBCI.ViewModels.Base;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Windowing;

namespace AvaloniaOpenBCI.Views;

public partial class MainWindow : AppWindowBase
{
    private readonly INotificationService _notificationService;
    private readonly INavigationService<MainWindowViewModel> _navigationService;

    private FlyoutBase? _progressFlyout;

    [DesignOnly(true)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public MainWindow()
    {
        _notificationService = null;
        _navigationService = null;
    }

    public MainWindow(
        INotificationService notificationService,
        INavigationService<MainWindowViewModel> navigationService
    )
    {
        _notificationService = notificationService;
        _navigationService = navigationService;

        InitializeComponent();

        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

        _navigationService.TypedNavigation += NavigationServiceOnTypedNavigation;

        EventManager.Instance.ToggleProgressFlyout += (_, _) => _progressFlyout?.Hide();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _navigationService.SetFrame(FrameView ?? throw new NullReferenceException("Frame not found"));
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        Application.Current!.ActualThemeVariantChanged += OnActualThemeVariantChanged;

        var theme = ActualThemeVariant;
        // Enable mica for windows11
        if (IsWindows11 && theme != FluentAvaloniaTheme.HighContrastTheme)
        {
            TryEnableMicaEffect();
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // Initialize notification service using this window as the visual root
        _notificationService.Initialize(this);

        // Attach error notification handler for image loader
        if (ImageLoader.AsyncImageLoader is FallbackRamCachedWebImageLoader loader)
        {
            loader.LoadFailed += OnImageLoadFailed;
        }

        if (DataContext is not MainWindowViewModel vm)
            return;

        // Navigate to first page
        Dispatcher.UIThread.Post(() =>
        {
            _navigationService.NavigateTo(
                vm.Pages[0],
                new BetterSlideNavigationTransition { Effect = SlideNavigationTransitionEffect.FromBottom }
            );
        });
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        // Detach error notification handler for image loader
        if (ImageLoader.AsyncImageLoader is FallbackRamCachedWebImageLoader loader)
        {
            loader.LoadFailed -= OnImageLoadFailed;
        }
    }

    private void OnImageLoadFailed(object? sender, ImageLoadFailedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var fileName = Path.GetFileName(e.Url);
            var displayName = string.IsNullOrEmpty(fileName) ? e.Url : fileName;
            _notificationService.Show(
                "Failed to load image",
                $"Could not load '{displayName}'\n{e.Exception.Message}",
                NotificationType.Warning
            );
        });
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        if (IsWindows11)
        {
            if (ActualThemeVariant != FluentAvaloniaTheme.HighContrastTheme)
            {
                TryEnableMicaEffect();
            }
            else
            {
                ClearValue(BackgroundProperty);
                ClearValue(TransparencyBackgroundFallbackProperty);
            }
        }
    }

    private void TryEnableMicaEffect()
    {
        TransparencyBackgroundFallback = Brushes.Transparent;
        TransparencyLevelHint = new[]
        {
            WindowTransparencyLevel.Mica,
            WindowTransparencyLevel.AcrylicBlur,
            WindowTransparencyLevel.Blur,
        };

        if (ActualThemeVariant == ThemeVariant.Dark)
        {
            var color = this.TryFindResource("SolidBackgroundFillColorBase", ThemeVariant.Dark, out var value)
                ? (Color2)(Color)value!
                : new Color2(30, 31, 34);

            color = color.LightenPercent(-0.5f);
            Background = new ImmutableSolidColorBrush(color, 0.72);
        }
        else
        {
            // Similar effect here
            var color = this.TryFindResource("SolidBackgroundFillColorBase", ThemeVariant.Light, out var value)
                ? (Color2)(Color)value!
                : new Color2(243, 243, 243);

            color = color.LightenPercent(0.5f);
            Background = new ImmutableSolidColorBrush(color, 0.9);
        }
    }

    private void NavigationServiceOnTypedNavigation(object? sender, TypedNavigationEventArgs e)
    {
        var mainViewModel = (MainWindowViewModel)DataContext!;

        mainViewModel.SelectedCategory = mainViewModel
            .Pages.Concat(mainViewModel.FooterPages)
            .FirstOrDefault(x => x.GetType() == e.ViewModelType);
    }

    private void NavigationView_OnItemInvoked(object? sender, NavigationViewItemInvokedEventArgs e)
    {
        if (e.InvokedItemContainer is NavigationViewItem nvi)
        {
            // Skip if this is the currently selected item
            if (nvi.IsSelected)
                return;

            if (nvi.Tag is null)
            {
                throw new InvalidOperationException("NavigationViewItem Tag is null");
            }

            if (nvi.Tag is not ViewModelBase vm)
            {
                throw new InvalidOperationException(
                    $"NavigationViewItem Tag must be of type ViewModelBase, not {nvi.Tag?.GetType()}"
                );
            }

            _navigationService.NavigateTo(vm, new BetterSlideNavigationTransition());
        }
    }
}
