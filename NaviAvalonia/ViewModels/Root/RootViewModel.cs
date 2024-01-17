using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using NaviAvalonia.Helper;
using NaviAvalonia.Routing;
using NaviAvalonia.Services.MainWindow;
using NaviAvalonia.Views;
using ReactiveUI;

namespace NaviAvalonia.ViewModels.Root;

public class RootViewModel : RoutableHostScreen<RoutableScreen>, IMainWindowProvider
{
    private readonly IRouter _router;
    private readonly IMainWindowService _mainWindowService;
    private readonly IClassicDesktopStyleApplicationLifetime _lifeTime;
    private readonly DefaultTitleBarViewModel _defaultTitleBarViewModel;
    private readonly ObservableAsPropertyHelper<ViewModelBase?> _titleBarViewModel;

    public bool IsMainWindowOpen => _lifeTime.MainWindow != null;
    public bool IsMainWindowFocused { get; private set; }

    public RootViewModel(
        IRouter router,
        IMainWindowService mainWindowService,
        DefaultTitleBarViewModel defaultTitleBarViewModel
    )
    {
        UI.SetMicaEnabled(false);

        _router = router;
        _mainWindowService = mainWindowService;
        _defaultTitleBarViewModel = defaultTitleBarViewModel;
        _lifeTime = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;

        _router.SetRoot(this);
        _mainWindowService.ConfigureMainWindowProvider(this);

        Dispatcher.UIThread.InvokeAsync(OpenMainWindow);

        _titleBarViewModel = this.WhenAnyValue(vm => vm.Screen)
            .Select(s => s as IMainScreenViewModel)
            .Select(s => s?.WhenAnyValue(svm => svm.TitleBarViewModel) ?? Observable.Never<ViewModelBase>())
            .Switch()
            .Select(vm => vm ?? _defaultTitleBarViewModel)
            .ToProperty(this, vm => vm.TitleBarViewModel);

        Task.Run(() =>
        {
            _router.Navigate("home");
        });
    }

    public ViewModelBase? TitleBarViewModel => _titleBarViewModel.Value;

    public void GoBack()
    {
        _router.GoBack();
    }

    public void GoForward()
    {
        _router.GoForward();
    }

    public void OpenMainWindow()
    {
        if (_lifeTime.MainWindow == null)
        {
            _lifeTime.MainWindow = new MainWindow { DataContext = this };
            _lifeTime.MainWindow.Show();
            _lifeTime.MainWindow.Closing += CurrentMainWindowOnClosing;
        }

        _lifeTime.MainWindow.Activate();
        if (_lifeTime.MainWindow.WindowState == WindowState.Minimized)
            _lifeTime.MainWindow.WindowState = WindowState.Maximized;

        OnMainWindowOpened();
    }

    private void CurrentMainWindowOnClosing(object? sender, WindowClosingEventArgs e)
    {
        _lifeTime.MainWindow = null;
        OnMainWindowClosed();
    }

    public void CloseMainWindow()
    {
        Dispatcher.UIThread.Post(() =>
        {
            _lifeTime.MainWindow?.Close();
        });
    }

    public void Focused()
    {
        IsMainWindowFocused = true;
        OnMainWindowFocused();
    }

    public void Unfocused()
    {
        IsMainWindowFocused = false;
        OnMainWindowUnfocused();
    }

    public event EventHandler? MainWindowOpened;
    public event EventHandler? MainWindowClosed;
    public event EventHandler? MainWindowFocused;
    public event EventHandler? MainWindowUnfocused;

    protected virtual void OnMainWindowOpened()
    {
        MainWindowOpened?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnMainWindowClosed()
    {
        MainWindowClosed?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnMainWindowFocused()
    {
        MainWindowFocused?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnMainWindowUnfocused()
    {
        MainWindowUnfocused?.Invoke(this, EventArgs.Empty);
    }
}
