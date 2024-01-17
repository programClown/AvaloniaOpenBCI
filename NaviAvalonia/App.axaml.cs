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
using NaviAvalonia.ViewModels;
using NaviAvalonia.Views;

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
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { DataContext = new MainWindowViewModel(), };
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
        DesktopLifetime.MainWindow = mainWindow;
    }

    private static void ConfigureServiceProvider()
    {
        IServiceCollection services = ConfigureServices();

        Services = services.BuildServiceProvider();
    }

    private static void ConfigureViewServices(IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainWindowViewModel>(provider => new MainWindowViewModel { });
    }

    private static IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();

        ConfigureViewServices(services);

        return services;
    }
}
