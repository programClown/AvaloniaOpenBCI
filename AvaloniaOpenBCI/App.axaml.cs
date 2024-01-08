using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using AvaloniaOpenBCI.Helper;
using AvaloniaOpenBCI.Models.Configs;
using AvaloniaOpenBCI.ViewModels;
using AvaloniaOpenBCI.Views;
using MessagePipe;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaOpenBCI;

public class App : Application
{
    private static bool _isAsyncDisposeComplete;

    public App() { }

    [NotNull]
    public static IServiceProvider? Services { get; internal set; }

    [NotNull]
    public static Visual? VisualRoot { get; internal set; }

    public static TopLevel TopLevel => TopLevel.GetTopLevel(VisualRoot)!;

    [NotNull]
    public static IStorageProvider? StorageProvider { get; internal set; }

    [NotNull]
    public static IClipboard? Clipboard { get; internal set; }

    // ReSharper disable once MemberCanBePrivate.Global
    [NotNull]
    public static IConfiguration? Config { get; internal set; }

    // ReSharper disable once MemberCanBePrivate.Global
    public IClassicDesktopStyleApplicationLifetime? DesktopLifetime =>
        ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

    /// <summary>
    ///     Called before <see cref="Services" /> is built.
    ///     Can be used by UI tests to override services.
    /// </summary>
    internal static event EventHandler<IServiceCollection>? BeforeBuildServiceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // Set design theme
        if (Design.IsDesignMode)
        {
            RequestedThemeVariant = ThemeVariant.Dark;
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Remove DataAnnotations validation plugin since we're using INotifyDataErrorInfo from MvvmToolKit
        var dataValidationPluginsToRemove = BindingPlugins
            .DataValidators.OfType<DataAnnotationsValidationPlugin>()
            .ToArray();

        foreach (DataAnnotationsValidationPlugin plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }

        base.OnFrameworkInitializationCompleted();

        ConfigureServiceProvider();

        // if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        // {
        //     desktop.MainWindow = new MainWindow { DataContext = new MainWindowViewModel() };
        // }

        if (DesktopLifetime is not null)
        {
            ShowMainWindow();
        }
    }

    private void ShowMainWindow()
    {
        if (DesktopLifetime is null)
            return;

        var mainViewModel = Services.GetRequiredService<MainWindowViewModel>();

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = mainViewModel;

        mainWindow.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
        mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

        VisualRoot = mainWindow;
        StorageProvider = mainWindow.StorageProvider;
        Clipboard = mainWindow.Clipboard ?? throw new NullReferenceException("Clipboard is null");

        DesktopLifetime.MainWindow = mainWindow;
        DesktopLifetime.Exit += OnExit;
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e) { }

    private static void ConfigureServiceProvider()
    {
        IServiceCollection services = ConfigureServices();

        BeforeBuildServiceProvider?.Invoke(null, services);

        Services = services.BuildServiceProvider();
    }

    internal static IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddLazyInstance();

        services.AddMessagePipe();
        services.AddMessagePipeNamedPipeInterprocess("AvaloniaOpenBCI");

        services.AddSingleton<MainWindow>();

        services.AddSingleton<MainWindowViewModel>();

        Config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .Build();

        services.Configure<DebugOptions>(Config.GetSection(nameof(DebugOptions)));

        if (Compat.IsWindows)
        {
            services.AddSingleton<IPrerequisiteHelper, WindowsPrerequisiteHelper>();
        }
        else if (Compat.IsLinux || Compat.IsMacOS)
        {
            services.AddSingleton<IPrerequisiteHelper, UnixPrerequisiteHelper>();
        }

        return services;
    }

    public static void Shutdown(int exitCode = 0)
    {
        if (Current is null)
        {
            throw new NullReferenceException("Current Application was null when Shutdown called");
        }

        if (Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.Shutdown(exitCode);
        }
    }
}
