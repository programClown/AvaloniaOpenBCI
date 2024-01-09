using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaloniaEdit.Document;
using AvaloniaOpenBCI.Attributes;
using AvaloniaOpenBCI.ViewModels.Base;
using AvaloniaOpenBCI.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.FluentAvalonia.SymbolIconSource;

namespace AvaloniaOpenBCI.ViewModels;

[View(typeof(LaunchPageView))]
public partial class LaunchPageViewModel : PageViewModelBase, IDisposable, IAsyncDisposable
{
    public override string Title => "Launch";
    public override IconSource IconSource => new SymbolIconSource { Symbol = Symbol.Rocket, IsFilled = true };

    [ObservableProperty]
    private bool _isLaunchTeachingTipsOpen;

    [ObservableProperty]
    private ObservableCollection<string> _installedPackages = new() { "one", "two", "three" };

    [ObservableProperty]
    private string? _selectedPackage;

    [ObservableProperty]
    private bool _autoScrollToEnd = true;

    [ObservableProperty]
    private bool _showManualInputPrompt = true;

    [ObservableProperty]
    private TextDocument document = new();

    public void Dispose()
    {
        // TODO 在此释放托管资源
    }

    public async ValueTask DisposeAsync()
    {
        // TODO 在此释放托管资源
    }

    public List<string> SelectedPackageExtraCommands => new() { "launch", "file", "async" };
}
