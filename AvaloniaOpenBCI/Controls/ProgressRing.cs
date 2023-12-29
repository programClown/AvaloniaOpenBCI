using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;

namespace AvaloniaOpenBCI.Controls;

/// <summary>
///     A control used to indicate the progress of an operation.
/// </summary>
[PseudoClasses(":preserveaspect", ":indeterminate")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class ProgressRing : RangeBase
{
    public readonly static StyledProperty<bool> IsIndeterminateProperty = ProgressBar
        .IsIndeterminateProperty
        .AddOwner<ProgressRing>();

    public readonly static StyledProperty<bool> PreserveAspectProperty = AvaloniaProperty.Register<ProgressRing, bool>(
        nameof(PreserveAspect),
        true
    );

    public readonly static StyledProperty<double> StrokeThicknessProperty = Shape
        .StrokeThicknessProperty
        .AddOwner<ProgressRing>();

    public readonly static StyledProperty<double> StartAngleProperty = AvaloniaProperty.Register<ProgressRing, double>(
        nameof(StartAngle)
    );

    public readonly static StyledProperty<double> SweepAngleProperty = AvaloniaProperty.Register<ProgressRing, double>(
        nameof(SweepAngle)
    );

    public readonly static StyledProperty<double> EndAngleProperty = AvaloniaProperty.Register<ProgressRing, double>(
        nameof(EndAngle),
        360
    );

    private Arc? _fillArc;

    static ProgressRing()
    {
        AffectsRender<ProgressRing>(SweepAngleProperty, StartAngleProperty, EndAngleProperty);

        ValueProperty.Changed.AddClassHandler<ProgressRing>(OnValuePropertyChanged);
        SweepAngleProperty.Changed.AddClassHandler<ProgressRing>(OnSweepAnglePropertyChanged);
    }

    public ProgressRing()
    {
        UpdatePseudoClasses(IsIndeterminate, PreserveAspect);
    }

    public bool IsIndeterminate
    {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    public bool PreserveAspect
    {
        get => GetValue(PreserveAspectProperty);
        set => SetValue(PreserveAspectProperty, value);
    }

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public double StartAngle
    {
        get => GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    public double SweepAngle
    {
        get => GetValue(SweepAngleProperty);
        set => SetValue(SweepAngleProperty, value);
    }

    public double EndAngle
    {
        get => GetValue(EndAngleProperty);
        set => SetValue(EndAngleProperty, value);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _fillArc = e.NameScope.Find<Arc>("PART_Fill");
        if (_fillArc is not null)
        {
            _fillArc.StartAngle = StartAngle;
            _fillArc.SweepAngle = SweepAngle;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        var e = change as AvaloniaPropertyChangedEventArgs<bool>;
        if (e is null)
            return;

        if (e.Property == IsIndeterminateProperty)
        {
            UpdatePseudoClasses(e.NewValue.GetValueOrDefault(), null);
        }
        else if (e.Property == PreserveAspectProperty)
        {
            UpdatePseudoClasses(null, e.NewValue.GetValueOrDefault());
        }
    }

    private void UpdatePseudoClasses(bool? isIndeterminate, bool? preserveAspect)
    {
        if (isIndeterminate.HasValue)
        {
            PseudoClasses.Set(":indeterminate", isIndeterminate.Value);
        }

        if (preserveAspect.HasValue)
        {
            PseudoClasses.Set(":preserveaspect", preserveAspect.Value);
        }
    }

    private static void OnValuePropertyChanged(ProgressRing sender, AvaloniaPropertyChangedEventArgs e)
    {
        sender.SweepAngle =
            ((double)e.NewValue! - sender.Minimum)
            * (sender.EndAngle - sender.StartAngle)
            / (sender.Maximum - sender.Minimum);
    }

    private static void OnSweepAnglePropertyChanged(ProgressRing sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender._fillArc is { } arc)
        {
            arc.SweepAngle = Math.Round(e.GetNewValue<double>());
        }
    }
}