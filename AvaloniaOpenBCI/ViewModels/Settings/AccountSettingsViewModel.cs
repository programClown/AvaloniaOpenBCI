using AvaloniaOpenBCI.Attributes;
using AvaloniaOpenBCI.ViewModels.Base;
using AvaloniaOpenBCI.Views.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.FluentAvalonia.SymbolIconSource;

namespace AvaloniaOpenBCI.ViewModels.Settings;

[View(typeof(AccountSettingsPage))]
public partial class AccountSettingsViewModel : PageViewModelBase
{
    public override string Title => "Accounts";
    public override IconSource IconSource => new SymbolIconSource { Symbol = Symbol.Person, IsFilled = true };

    [ObservableProperty]
    private string? _accountProfileImageUrl;
}
