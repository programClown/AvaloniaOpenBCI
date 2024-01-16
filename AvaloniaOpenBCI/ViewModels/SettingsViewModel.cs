using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvaloniaEdit.Utils;
using AvaloniaOpenBCI.Attributes;
using AvaloniaOpenBCI.ViewModels.Base;
using AvaloniaOpenBCI.ViewModels.Settings;
using AvaloniaOpenBCI.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
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

    public SettingsViewModel()
    {
        SubPages = new PageViewModelBase[]
        {
            App.Services.GetRequiredService<MainSettingsViewModel>(),
            App.Services.GetRequiredService<AccountSettingsViewModel>()
        };

        CurrentPagePath.AddRange(SubPages);

        CurrentPage = SubPages[0];
    }

    partial void OnCurrentPageChanged(PageViewModelBase? value)
    {
        if (value is null)
        {
            return;
        }

        if (value is MainSettingsViewModel)
        {
            CurrentPagePath.Clear();
            CurrentPagePath.Add(value);
        }
        else
        {
            CurrentPagePath.Clear();
            CurrentPagePath.AddRange(new[] { SubPages[0], value });
        }
    }
}
