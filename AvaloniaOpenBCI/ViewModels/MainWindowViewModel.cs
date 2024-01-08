using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using AvaloniaOpenBCI.Helper;
using AvaloniaOpenBCI.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;

namespace AvaloniaOpenBCI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private PageViewModelBase? _currentPage;

    [ObservableProperty]
    private object? _selectedCategory;

    [ObservableProperty]
    private List<PageViewModelBase> _pages = new();

    [ObservableProperty]
    private List<PageViewModelBase> _footerPages = new();

    public MainWindowViewModel() { }

    public override void OnLoaded()
    {
        base.OnLoaded();

        // Set only if null, since this maybe called again when content dialogs open
        CurrentPage ??= Pages.FirstOrDefault();
        SelectedCategory ??= Pages.FirstOrDefault();
    }

    protected override async Task OnInitialLoadedAsync()
    {
        await base.OnLoadedAsync();

        if (Design.IsDesignMode)
            return;

        Program.StartupTimer.Stop();
        var startupTime = CodeTimer.FormatTime(Program.StartupTimer.Elapsed);
        Log.Logger.Information($"App started {startupTime}");
    }
}
