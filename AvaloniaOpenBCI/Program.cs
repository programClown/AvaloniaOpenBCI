using AsyncImageLoader;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using AvaloniaOpenBCI.Helper;
using AvaloniaOpenBCI.ViewModels.Dialogs;
using AvaloniaOpenBCI.Views.Dialogs;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Semver;
using Serilog;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaOpenBCI;

internal sealed class Program
{
    private static readonly bool IsDebugExceptionDialog = false;
    public static bool IsDebugBuild { get; private set; }
    public static Stopwatch StartupTimer { get; } = new Stopwatch();

    public static bool UseOpenGlRendering { get; } = false;

    public static bool DisableGpuRendering { get; } = false;

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

        LiveCharts.Configure(config =>                                                // mark
                config.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('汉')) // <- Chinese // mark
        );                                                                            // mark

        string? infoVersion = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        Compat.AppVersion = SemVersion.Parse(infoVersion ?? "0.0.0", SemVersionStyles.Strict);

        // Configure exception dialog for unhandled exceptions
        if (!Debugger.IsAttached || IsDebugExceptionDialog)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    internal static void SetupAvaloniaApp()
    {
        IconProvider.Current.Register<FontAwesomeIconProvider>();
        // Use our custom image loader for custom local load error handling
        ImageLoader.AsyncImageLoader.Dispose();
        ImageLoader.AsyncImageLoader = new FallbackRamCachedWebImageLoader();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        SetupAvaloniaApp();

        AppBuilder app = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

        if (UseOpenGlRendering)
        {
            app = app.With(
                new Win32PlatformOptions
                {
                    RenderingMode = [Win32RenderingMode.Wgl, Win32RenderingMode.Software]
                }
            );
        }

        if (DisableGpuRendering)
        {
            app = app.With(new Win32PlatformOptions
                {
                    RenderingMode = new[]
                    {
                        Win32RenderingMode.Software
                    }
                }
            ).With(new X11PlatformOptions
                {
                    RenderingMode = new[]
                    {
                        X11RenderingMode.Software
                    }
                }
            ).With(new AvaloniaNativePlatformOptions
                {
                    RenderingMode = new[]
                    {
                        AvaloniaNativeRenderingMode.Software
                    }
                }
            );
        }

        return app;
    }


    private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        if (e.Exception is Exception ex)
        {
            Log.Logger.Error(ex, "Unobserved task exception");
        }
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
            var dialog = new ExceptionDialog { DataContext = new ExceptionViewModel { Exception = ex } };

            Window? mainWindow = lifetime.MainWindow;
            // We can only show dialog if main window exists, and is visible
            if (mainWindow is { PlatformImpl: not null, IsVisible: true })
            {
                // Configure for dialog mode
                dialog.ShowAsDialog = true;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                // Show synchronously without blocking UI thread
                // https://github.com/AvaloniaUI/Avalonia/issues/4810#issuecomment-704259221
                var cts = new CancellationTokenSource();

                dialog
                    .ShowDialog(mainWindow)
                    .ContinueWith(
                        _ =>
                        {
                            cts.Cancel();
                            ExitWithException(ex);
                        },
                        TaskScheduler.FromCurrentSynchronizationContext()
                    );

                Dispatcher.UIThread.MainLoop(cts.Token);
            }
            else
            {
                // No parent window available
                var cts = new CancellationTokenSource();
                // Exit on token cancellation
                cts.Token.Register(() => ExitWithException(ex));

                dialog.ShowWithCts(cts);

                Dispatcher.UIThread.MainLoop(cts.Token);
            }
        }
    }

    [DoesNotReturn]
    private static void ExitWithException(Exception exception)
    {
        App.Shutdown(1);
        Dispatcher.UIThread.InvokeShutdown();
        Environment.Exit(Marshal.GetHRForException(exception));
    }

    [Conditional("DEBUG")]
    private static void SetDebugBuild()
    {
        IsDebugBuild = true;
    }
}