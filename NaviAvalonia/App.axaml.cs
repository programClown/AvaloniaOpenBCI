using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using NaviAvalonia.Routing;
using NaviAvalonia.Services.MainWindow;
using NaviAvalonia.ViewModels;
using NaviAvalonia.ViewModels.Home;
using NaviAvalonia.ViewModels.Root;
using NaviAvalonia.ViewModels.Settings;
using NaviAvalonia.Views;
using NaviAvalonia.Views.Home;
using NaviAvalonia.Views.Root;
using NaviAvalonia.Views.Settings;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace NaviAvalonia;

public partial class App : Application
{
    [NotNull]
    public static IServiceProvider? Services { get; internal set; }

    [NotNull]
    public static Visual? VisualRoot { get; internal set; }

    public static TopLevel TopLevel => TopLevel.GetTopLevel(VisualRoot)!;

    public IClassicDesktopStyleApplicationLifetime? DesktopLifetime =>
        ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

    internal static readonly LoggingLevelSwitch LoggingLevelSwitch = new(LogEventLevel.Verbose);

    internal static readonly ILogger Logger = new LoggerConfiguration()
        .WriteTo.File(
            "logs/log.txt",
            fileSizeLimitBytes: 5 * 1024 * 1024,
            rollOnFileSizeLimit: true,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
        )
#if DEBUG
        .WriteTo.Debug()
#endif
        .MinimumLevel.ControlledBy(LoggingLevelSwitch)
        .CreateLogger();

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

        var rootViewModel = Services.GetRequiredService<RootViewModel>();
        this.DataContext = rootViewModel;
    }

    private static void ConfigureServiceProvider()
    {
        IServiceCollection services = ConfigureServices();

        Services = services.BuildServiceProvider();
        Program.CreateLogger(Services);
    }

    private static void ConfigureViewServices(IServiceCollection services)
    {
        services.AddSingleton<IMainWindowService, MainWindowService>();
        services.AddSingleton<BlankView>();
        services.AddSingleton<BlankViewModel>();
        services.AddSingleton<DefaultTitleBarView>();
        services.AddSingleton<DefaultTitleBarViewModel>();
        services.AddSingleton<RootView>();
        services.AddSingleton<RootViewModel>();
        services.AddSingleton<HomeView>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<SettingsView>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<MainWindow>();
    }

    private static IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILogger>(Logger);
        services.AddSingleton<IRouter, Router>();

        ConfigureViewServices(services);

        return services;
    }
}
