using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Styling;
using Avalonia.Threading;
using AvaloniaOpenBCI.Animations;
using AvaloniaOpenBCI.Attributes;
using AvaloniaOpenBCI.Controls;
using AvaloniaOpenBCI.Extensions;
using AvaloniaOpenBCI.Helper;
using AvaloniaOpenBCI.Helper.HardwareInfo;
using AvaloniaOpenBCI.Models.FileInterfaces;
using AvaloniaOpenBCI.Services;
using AvaloniaOpenBCI.ViewModels.Base;
using AvaloniaOpenBCI.Views.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Serilog;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.FluentAvalonia.SymbolIconSource;

namespace AvaloniaOpenBCI.ViewModels.Settings;

[View(typeof(MainSettingsPage))]
public partial class MainSettingsViewModel : PageViewModelBase
{
    private readonly INotificationService _notificationService;
    private readonly INavigationService<SettingsViewModel> _settingsNavigationService;
    public override string Title => "Settings";
    public override IconSource IconSource => new SymbolIconSource { Symbol = Symbol.Settings, IsFilled = true };

    [ObservableProperty]
    private string? _selectedTheme;
    public IReadOnlyList<string> AvailableThemes => new[] { "Light", "Dark", "System", };

    [ObservableProperty]
    private CultureInfo _selectedLanguage;
    public IReadOnlyList<CultureInfo> AvailableLanguages => new[] { new CultureInfo("zh-Hans") };

    [ObservableProperty]
    private string _holidayModeSetting = "Automatic";
    public IReadOnlyList<string> HolidayModes => new[] { "Automatic", "Enabled", "Disabled", };

    private static Lazy<IReadOnlyList<GpuInfo>> GpuInfosLazy { get; } =
        new(() => HardwareHelper.IterGpuInfo().ToImmutableArray());

    public static IReadOnlyList<GpuInfo> GpuInfos => GpuInfosLazy.Value;

    [ObservableProperty]
    private MemoryInfo _memoryInfo;

    public Task<CpuInfo> CpuInfoAsync => HardwareHelper.GetCpuInfoAsync();

    //info Section
    private const int VersionTapCountThreshold = 7;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(VersionFlyoutText))]
    private int _versionTapCount;

    [ObservableProperty]
    private bool _isVersionTapTeachingTipOpen;
    public string VersionFlyoutText =>
        $"You are {VersionTapCountThreshold - VersionTapCount} clicks away from enabling Debug options.";

    // ReSharper disable once MemberCanBeMadeStatic.Global
    public string AppVersion =>
        $"Version {Compat.AppVersion.ToDisplayString()}" + (Program.IsDebugBuild ? " (Debug)" : "");

    public MainSettingsViewModel(
        INotificationService notificationService,
        INavigationService<SettingsViewModel> settingsNavigationService
    )
    {
        _notificationService = notificationService;
        _settingsNavigationService = settingsNavigationService;

        SelectedTheme = AvailableThemes.Last();
        SelectedLanguage = AvailableLanguages[0];
    }

    partial void OnSelectedThemeChanged(string? value)
    {
        if (Application.Current is null)
            return;

        Application.Current.RequestedThemeVariant = value switch
        {
            "Dark" => ThemeVariant.Dark,
            "Light" => ThemeVariant.Light,
            _ => ThemeVariant.Default
        };
    }

    [RelayCommand]
    private async Task OpenEnvVarsDialog()
    {
        var dialog = new BetterContentDialog()
        {
            Content = "Env Variables",
            PrimaryButtonText = "保存",
            IsPrimaryButtonEnabled = true,
            CloseButtonText = "取消"
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            ;
        }
    }

    /// <summary>
    /// Adds Application to Start Menu for the current user.
    /// </summary>
    [RelayCommand]
    private async Task AddToStartMenu()
    {
        if (!Compat.IsWindows)
        {
            _notificationService.Show("Not supported", "This feature is only supported on Windows.");
            return;
        }

        await using var _ = new MinimumDelay(200, 300);

        var shortcutDir = new DirectoryPath(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
        var shortcutLink = shortcutDir.JoinFile("AvaloniaOpenBCI.lnk");

        var appPath = Compat.AppCurrentPath;
        var iconPath = shortcutDir.JoinFile("AvaloniaOpenBCI.ico");
        await Assets.AppIcon.ExtractTo(iconPath);

        WindowsShortcuts.CreateShortcut(shortcutLink, appPath, iconPath, "AvaloniaOpenBCI");

        _notificationService.Show(
            "Added to Start Menu",
            "AvaloniaOpenBCI has been added to the Start Menu.",
            NotificationType.Success
        );
    }

    /// <summary>
    /// Add Application to Start Menu for all users.
    /// <remarks>Requires Admin elevation.</remarks>
    /// </summary>
    [RelayCommand]
    private async Task AddToGlobalStartMenu()
    {
        if (!Compat.IsWindows)
        {
            _notificationService.Show("Not supported", "This feature is only supported on Windows.");
            return;
        }

        // Confirmation dialog
        var dialog = new BetterContentDialog
        {
            Title = "This will create a shortcut for AvaloniaOpenBCI in the Start Menu for all users",
            Content = "You will be prompted for administrator privileges. Continue?",
            PrimaryButtonText = "Yes",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };

        if (await dialog.ShowAsync() != ContentDialogResult.Primary)
        {
            return;
        }

        await using var _ = new MinimumDelay(200, 300);

        var shortcutDir = new DirectoryPath(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
            "Programs"
        );
        var shortcutLink = shortcutDir.JoinFile("AvaloniaOpenBCI.lnk");

        var appPath = Compat.AppCurrentPath;
        var iconPath = shortcutDir.JoinFile("AvaloniaOpenBCI.ico");

        // We can't directly write to the targets, so extract to temporary directory first
        using var tempDir = new TempDirectoryPath();

        await Assets.AppIcon.ExtractTo(tempDir.JoinFile("AvaloniaOpenBCI.ico"));
        WindowsShortcuts.CreateShortcut(tempDir.JoinFile("AvaloniaOpenBCI.lnk"), appPath, iconPath, "AvaloniaOpenBCI");

        // Move to target
        try
        {
            var moveLinkResult = await WindowsElevated.MoveFiles(
                (tempDir.JoinFile("AvaloniaOpenBCI.lnk"), shortcutLink),
                (tempDir.JoinFile("AvaloniaOpenBCI.ico"), iconPath)
            );
            if (moveLinkResult != 0)
            {
                _notificationService.ShowPersistent(
                    "Failed to create shortcut",
                    $"Could not copy shortcut",
                    NotificationType.Error
                );
            }
        }
        catch (Win32Exception e)
        {
            // We'll get this exception if user cancels UAC
            Log.Logger.Warning(e, "Could not create shortcut");
            _notificationService.Show("Could not create shortcut", "", NotificationType.Warning);
            return;
        }

        _notificationService.Show(
            "Added to Start Menu",
            "Stability Matrix has been added to the Start Menu for all users.",
            NotificationType.Success
        );
    }

    public void OnVersionClick()
    {
        VersionTapCount++;

        switch (VersionTapCount)
        {
            // Reached required threshold
            case >= VersionTapCountThreshold:
            {
                IsVersionTapTeachingTipOpen = false;
                // Enable debug options
                _notificationService.Show(
                    "Debug options enabled",
                    "Warning: Improper use may corrupt application state or cause loss of data."
                );
                VersionTapCount = 0;
                break;
            }
            // Open teaching tip above 3rd click
            case >= 3:
                IsVersionTapTeachingTipOpen = true;
                break;
        }
    }

    [RelayCommand]
    private async Task ShowLicensesDialog()
    {
        try
        {
            var markdown = GetLicensesMarkdown();

            var dialog = DialogHelper.CreateMarkdownDialog(markdown, "Licenses");
            dialog.MaxDialogHeight = 600;
            await dialog.ShowAsync();
        }
        catch (Exception e)
        {
            _notificationService.Show("Failed to read licenses information", $"{e}", NotificationType.Error);
        }
    }

    [RelayCommand]
    public void NavigateToSubPageCommand(Type viewModelType)
    {
        Dispatcher.UIThread.Post(
            () =>
                _settingsNavigationService.NavigateTo(
                    viewModelType,
                    BetterSlideNavigationTransition.PageSlideFromRight
                ),
            DispatcherPriority.Send
        );
    }

    private static string GetLicensesMarkdown()
    {
        // Generate markdown
        var builder = new StringBuilder();
        builder.AppendLine($"## MIT License");
        builder.AppendLine();
        return builder.ToString();
    }
}
