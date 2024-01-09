using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvaloniaOpenBCI.Attributes;
using AvaloniaOpenBCI.ViewModels.Base;
using AvaloniaOpenBCI.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.FluentAvalonia.SymbolIconSource;

namespace AvaloniaOpenBCI.ViewModels;

[View(typeof(SettingsPage))]
public partial class SettingsViewModel : PageViewModelBase
{
    public override string Title => "Settings";

    public override IconSource IconSource => new SymbolIconSource { Symbol = Symbol.Settings, IsFilled = true };

    public IReadOnlyList<PageViewModelBase> SubPages { get; }

    [ObservableProperty]
    private ObservableCollection<PageViewModelBase> _currentPagePath = [];

    [ObservableProperty]
    private PageViewModelBase? _currentPage;

    public SettingsViewModel() { }
}
