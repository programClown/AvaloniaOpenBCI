using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaloniaEdit.Document;
using AvaloniaOpenBCI.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;

namespace AvaloniaOpenBCI.ViewModels;

public partial class LaunchPageViewModel : PageViewModelBase, IDisposable, IAsyncDisposable
{
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

    public override string Title { get; }
    public override IconSource IconSource { get; }

    public List<string> SelectedPackageExtraCommands => new() { "launch", "file", "async" };
}
