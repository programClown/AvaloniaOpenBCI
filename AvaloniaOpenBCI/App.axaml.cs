using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using AvaloniaOpenBCI.Attributes;
using AvaloniaOpenBCI.Helper;
using AvaloniaOpenBCI.ViewModels;
using AvaloniaOpenBCI.Views;
using MessagePipe;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AvaloniaOpenBCI;

public class App : Application
{
    private static bool _isAsyncDisposeComplete;

    [NotNull]
    public static IServiceProvider? Services { get; internal set; }

    [NotNull]
    public static Visual? VisualRoot { get; internal set; }

    public static TopLevel TopLevel
    {
        get => TopLevel.GetTopLevel(VisualRoot)!;
    }

    [NotNull]
    public static IStorageProvider? StorageProvider { get; internal set; }

    [NotNull]
    public static IClipboard? Clipboard { get; internal set; }

    // ReSharper disable once MemberCanBePrivate.Global
    [NotNull]
    public static IConfiguration? Config { get; internal set; }

    // ReSharper disable once MemberCanBePrivate.Global
    public IClassicDesktopStyleApplicationLifetime? DesktopLifetime
    {
        get => ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
    }

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
            .DataValidators
            .OfType<DataAnnotationsValidationPlugin>()
            .ToArray();

        foreach (DataAnnotationsValidationPlugin plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }

        base.OnFrameworkInitializationCompleted();


        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
        }
    }

    private static void ConfigureServiceProvider()
    {
        IServiceCollection services = ConfigureServices();
    }

    internal static IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddLazyInstance();

        services.AddMessagePipe();
        services.AddMessagePipeNamedPipeInterprocess("AvaloniaOpenBCI");

        var exportedTypes = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .Where(a => a.FullName?.StartsWith("AvaloniaOpenBCI") == true)
            .SelectMany(a => a.GetExportedTypes())
            .ToArray();

        var transientTypes = exportedTypes
            .Select(t => new { t, attributes = t.GetCustomAttributes(typeof(TransientAttribute), false) })
            .Where(
                t1 => t1.attributes is { Length: > 0 } &&
                      !t1.t.Name.Contains("Mock", StringComparison.OrdinalIgnoreCase))
            .Select(t1 => new { Type = t1.t, Attribute = (TransientAttribute)t1.attributes[0] });
        
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