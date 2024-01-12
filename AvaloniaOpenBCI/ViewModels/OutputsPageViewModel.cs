using AvaloniaOpenBCI.Attributes;
using AvaloniaOpenBCI.ViewModels.Base;
using AvaloniaOpenBCI.Views;
using FluentAvalonia.UI.Controls;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.FluentAvalonia.SymbolIconSource;

namespace AvaloniaOpenBCI.ViewModels;

[View(typeof(OutputsPage))]
public class OutputsPageViewModel : PageViewModelBase
{
    public override string Title => "Outputs";

    public override IconSource IconSource => new SymbolIconSource { Symbol = Symbol.Grid, IsFilled = true };

    public OutputsPageViewModel() { }
}
