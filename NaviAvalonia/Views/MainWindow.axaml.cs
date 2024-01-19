using System;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using NaviAvalonia.ViewModels.Root;
using ReactiveUI;
using Serilog;

namespace NaviAvalonia.Views;

public partial class MainWindow : ReactiveAppWindow<RootViewModel>
{
    private bool _activated;
    private IDisposable? _positionObserver;

    public MainWindow()
    {
        Opened += OnOpened;
        Closed += OnClosed;
        Activated += OnActivated;
        Deactivated += OnDeactivated;
        InitializeComponent();

        _activated = true;
        RootPanel.LayoutUpdated += OnLayoutUpdated;
    }

    private void OnLayoutUpdated(object? sender, EventArgs e) { }

    private void OnDeactivated(object? sender, EventArgs e)
    {
        ViewModel?.Unfocused();
    }

    private void OnActivated(object? sender, EventArgs e)
    {
        ViewModel?.Focused();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _positionObserver?.Dispose();
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        TitleBar.ExtendsContentIntoTitleBar = true;

        _positionObserver = Observable
            .FromEventPattern<PixelPointEventArgs>(x => PositionChanged += x, x => PositionChanged -= x)
            .Select(_ => Unit.Default)
            .Merge(
                this.WhenAnyValue(vm => vm.WindowState, vm => vm.Width, vm => vm.Width, vm => vm.Height)
                    .Select(_ => Unit.Default)
            )
            .Throttle(TimeSpan.FromMilliseconds(200), AvaloniaScheduler.Instance)
            .Subscribe(_ => SaveWindowSize());
    }

    private void SaveWindowSize()
    {
        App.Services.GetRequiredService<ILogger>()
            .Information(
                $"Top: {this.Position.Y}, Left: {this.Position.X}, height: {this.Height}, width: {this.Width}"
            );
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.XButton1)
            ViewModel?.GoBack();
        else if (e.InitialPressMouseButton == MouseButton.XButton2)
            ViewModel?.GoForward();
    }
}
