﻿using AsyncAwaitBestPractices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaOpenBCI.ViewModels.Base;
using FluentAvalonia.UI.Controls;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace AvaloniaOpenBCI.Controls;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public class BetterContentDialog : ContentDialog
{
    public static readonly StyledProperty<bool> IsFooterVisibleProperty =
        AvaloniaProperty.Register<BetterContentDialog, bool>(
            "IsFooterVisible", true);

    public static readonly StyledProperty<ScrollBarVisibility> ContentVerticalScrollBarVisibilityProperty =
        AvaloniaProperty.Register<BetterContentDialog, ScrollBarVisibility>(
            "ContentScrollBarVisibility",
            ScrollBarVisibility.Auto
        );

    public static readonly StyledProperty<double> MinDialogWidthProperty =
        AvaloniaProperty.Register<BetterContentDialog, double>("MinDialogWidth");

    public static readonly StyledProperty<double> MaxDialogWidthProperty =
        AvaloniaProperty.Register<BetterContentDialog, double>("MaxDialogWidth");

    public static readonly StyledProperty<double> MaxDialogHeightProperty =
        AvaloniaProperty.Register<BetterContentDialog, double>("MaxDialogHeight");

    public static readonly StyledProperty<Thickness> ContentMarginProperty =
        AvaloniaProperty.Register<BetterContentDialog, Thickness>("ContentMargin");

    public static readonly StyledProperty<bool> CloseOnClickOutsideProperty =
        AvaloniaProperty.Register<BetterContentDialog, bool>("CloseOnClickOutside");

    private FABorder? _backgroundPart;

    public BetterContentDialog()
    {
        AddHandler(LoadedEvent, OnLoaded);
    }

    protected override Type StyleKeyOverride { get; } = typeof(ContentDialog);

    public bool IsFooterVisible
    {
        get => GetValue(IsFooterVisibleProperty);
        set => SetValue(IsFooterVisibleProperty, value);
    }

    public ScrollBarVisibility ContentVerticalScrollBarVisibility
    {
        get => GetValue(ContentVerticalScrollBarVisibilityProperty);
        set => SetValue(ContentVerticalScrollBarVisibilityProperty, value);
    }

    public double MinDialogWidth
    {
        get => GetValue(MinDialogWidthProperty);
        set => SetValue(MinDialogWidthProperty, value);
    }

    public double MaxDialogWidth
    {
        get => GetValue(MaxDialogWidthProperty);
        set => SetValue(MaxDialogWidthProperty, value);
    }

    public double MaxDialogHeight
    {
        get => GetValue(MaxDialogHeightProperty);
        set => SetValue(MaxDialogHeightProperty, value);
    }

    public Thickness ContentMargin
    {
        get => GetValue(ContentMarginProperty);
        set => SetValue(ContentMarginProperty, value);
    }

    /// <summary>
    ///     Whether to close the dialog when clicking outside of it (on the blurred background)
    /// </summary>
    public bool CloseOnClickOutside
    {
        get => GetValue(CloseOnClickOutsideProperty);
        set => SetValue(CloseOnClickOutsideProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (CloseOnClickOutside)
        {
            if (e.Source is Popup || _backgroundPart is null)
            {
                return;
            }

            Point point = e.GetPosition(this);

            if (
                !_backgroundPart.Bounds.Contains(point) &&
                (Content as Control)?.DataContext is ContentDialogViewModelBase vm
            )
            {
                vm.OnCloseButtonClick();
            }
        }
    }

    private void TryBindButtons()
    {
        if ((Content as Control)?.DataContext is ContentDialogViewModelBase viewModel)
        {
            viewModel.PrimaryButtonClick += OnDialogButtonClick;
            viewModel.SecondaryButtonClick += OnDialogButtonClick;
            viewModel.CloseButtonClick += OnDialogButtonClick;
        }
        else if (Content is ContentDialogViewModelBase viewModelDirect)
        {
            viewModelDirect.PrimaryButtonClick += OnDialogButtonClick;
            viewModelDirect.SecondaryButtonClick += OnDialogButtonClick;
            viewModelDirect.CloseButtonClick += OnDialogButtonClick;
        }
        else if (
            (Content as Control)?.DataContext
            is ContentDialogProgressViewModelBase progressViewModel
        )
        {
            progressViewModel.PrimaryButtonClick += OnDialogButtonClick;
            progressViewModel.SecondaryButtonClick += OnDialogButtonClick;
            progressViewModel.CloseButtonClick += OnDialogButtonClick;
        }

        // If commands provided, bind OnCanExecuteChanged to hide buttons
        // otherwise link visibility to IsEnabled
        if (PrimaryButton is not null)
        {
            if (PrimaryButtonCommand is not null)
            {
                PrimaryButtonCommand.CanExecuteChanged += (_, _) =>
                    PrimaryButton.IsEnabled = PrimaryButtonCommand.CanExecute(null);
                // Also set initial state
                PrimaryButton.IsEnabled = PrimaryButtonCommand.CanExecute(null);
            }
            else
            {
                PrimaryButton.IsVisible =
                    IsPrimaryButtonEnabled && !string.IsNullOrEmpty(PrimaryButtonText);
            }
        }

        if (SecondaryButton is not null)
        {
            if (SecondaryButtonCommand is not null)
            {
                SecondaryButtonCommand.CanExecuteChanged += (_, _) =>
                    SecondaryButton.IsEnabled = SecondaryButtonCommand.CanExecute(null);
                // Also set initial state
                SecondaryButton.IsEnabled = SecondaryButtonCommand.CanExecute(null);
            }
            else
            {
                SecondaryButton.IsVisible =
                    IsSecondaryButtonEnabled && !string.IsNullOrEmpty(SecondaryButtonText);
            }
        }

        if (CloseButton is not null)
        {
            if (CloseButtonCommand is not null)
            {
                CloseButtonCommand.CanExecuteChanged += (_, _) =>
                    CloseButton.IsEnabled = CloseButtonCommand.CanExecute(null);
                // Also set initial state
                CloseButton.IsEnabled = CloseButtonCommand.CanExecute(null);
            }
        }
    }

    protected void OnDialogButtonClick(object? sender, ContentDialogResult e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Result = e;
            HideCore();
        });
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        TryBindButtons();
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _backgroundPart = e.NameScope.Find<FABorder>("BackgroundElement");
        if (_backgroundPart is not null)
        {
            _backgroundPart.Margin = ContentMargin;
        }
    }

    private void OnLoaded(object? sender, RoutedEventArgs? e)
    {
        TryBindButtons();

        // Find the named grid
        // https://github.com/amwx/FluentAvalonia/blob/master/src/FluentAvalonia/Styling/
        // ControlThemes/FAControls/ContentDialogStyles.axaml#L96
        var border = VisualChildren[0] as Border;
        var panel = border?.Child as Panel;
        var faBorder = panel?.Children[0] as FABorder;

        // Set dialog bounds
        if (MaxDialogWidth > 0)
        {
            faBorder!.MaxWidth = MaxDialogWidth;
        }

        if (MinDialogWidth > 0)
        {
            faBorder!.MinWidth = MinDialogWidth;
        }
        if (MaxDialogHeight > 0)
        {
            faBorder!.MaxHeight = MaxDialogHeight;
        }

        var border2 = faBorder?.Child as Border;
        // Named Grid 'DialogSpace'
        if (border2?.Child is not Grid dialogSpaceGrid)
        {
            throw new InvalidOperationException("Could not find DialogSpace grid");
        }

        var scrollViewer = dialogSpaceGrid.Children[0] as ScrollViewer;
        var actualBorder = dialogSpaceGrid.Children[1] as Border;

        // Get the parent border, which is what we want to hide
        if (scrollViewer is null || actualBorder is null)
        {
            throw new InvalidOperationException("Could not find parent border");
        }

        var subBorder = scrollViewer.Content as Border;
        var subGrid = subBorder?.Child as Grid;
        if (subGrid is null)
        {
            throw new InvalidOperationException("Could not find sub grid");
        }
        var contentControlTitle = subGrid.Children[0] as ContentControl;
        // Hide title if empty
        if (Title is null or string { Length: 0 })
        {
            contentControlTitle!.IsVisible = false;
        }

        // Set footer and scrollbar visibility states
        actualBorder.IsVisible = IsFooterVisible;
        scrollViewer.VerticalScrollBarVisibility = ContentVerticalScrollBarVisibility;

        // Also call the vm's OnLoad
        if (Content is Control { DataContext: ViewModelBase viewModel })
        {
            viewModel.OnLoaded();
            Dispatcher.UIThread.InvokeAsync(viewModel.OnLoadedAsync).SafeFireAndForget();
        }
    }

    #region Reflection Shenanigans for setting content dialog result

    [NotNull]
    protected static readonly FieldInfo? ResultField = typeof(ContentDialog).GetField(
        "_result",
        BindingFlags.Instance | BindingFlags.NonPublic
    );

    protected ContentDialogResult Result
    {
        get => (ContentDialogResult)ResultField.GetValue(this)!;
        set => ResultField.SetValue(this, value);
    }

    [NotNull]
    protected static readonly MethodInfo? HideCoreMethod = typeof(ContentDialog).GetMethod(
        "HideCore",
        BindingFlags.Instance | BindingFlags.NonPublic
    );

    protected void HideCore()
    {
        HideCoreMethod.Invoke(this, null);
    }

    // Also get button properties to hide on command execution change
    [NotNull]
    protected static readonly FieldInfo? PrimaryButtonField = typeof(ContentDialog).GetField(
        "_primaryButton",
        BindingFlags.Instance | BindingFlags.NonPublic
    );

    protected Button? PrimaryButton
    {
        get => (Button?)PrimaryButtonField.GetValue(this)!;
        set => PrimaryButtonField.SetValue(this, value);
    }

    [NotNull]
    protected static readonly FieldInfo? SecondaryButtonField = typeof(ContentDialog).GetField(
        "_secondaryButton",
        BindingFlags.Instance | BindingFlags.NonPublic
    );

    protected Button? SecondaryButton
    {
        get => (Button?)SecondaryButtonField.GetValue(this)!;
        set => SecondaryButtonField.SetValue(this, value);
    }

    [NotNull]
    protected static readonly FieldInfo? CloseButtonField = typeof(ContentDialog).GetField(
        "_closeButton",
        BindingFlags.Instance | BindingFlags.NonPublic
    );

    protected Button? CloseButton
    {
        get => (Button?)CloseButtonField.GetValue(this)!;
        set => CloseButtonField.SetValue(this, value);
    }

    static BetterContentDialog()
    {
        if (ResultField is null)
        {
            throw new NullReferenceException("ResultField was not resolved");
        }
        if (HideCoreMethod is null)
        {
            throw new NullReferenceException("HideCoreMethod was not resolved");
        }
        if (PrimaryButtonField is null || SecondaryButtonField is null || CloseButtonField is null)
        {
            throw new NullReferenceException("Button fields were not resolved");
        }
    }

    #endregion
}