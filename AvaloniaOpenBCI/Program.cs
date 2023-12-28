using System;
using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaOpenBCI.Helper;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Semver;
using Serilog;
using SkiaSharp;

namespace AvaloniaOpenBCI;

internal sealed class Program
{
    readonly private static bool _isDebugExceptionDialog = false;
    public static bool IsDebugBuild { get; private set; }
    public static Stopwatch StartupTimer { get; } = new();

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        StartupTimer.Start();

        SetDebugBuild();

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Debug()
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
            .CreateLogger();

        LiveCharts.Configure(config => // mark
                config.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('汉')) // <- Chinese // mark
        ); // mark

        var infoVersion = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        Compat.AppVersion = SemVersion.Parse(infoVersion ?? "0.0.0", SemVersionStyles.Strict);

        // Configure exception dialog for unhandled exceptions
        if (!Debugger.IsAttached || _isDebugExceptionDialog)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }


    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    [Conditional("DEBUG")]
    private static void SetDebugBuild()
    {
        IsDebugBuild = true;
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is not Exception ex)
        {
            return;
        }

        Log.Logger.Fatal(ex, "Unhandled {Type}: {Message}", ex.GetType().Name, ex.Message);

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
        }
    }
}