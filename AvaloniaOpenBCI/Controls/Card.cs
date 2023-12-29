using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AvaloniaOpenBCI.Controls;

public class Card : UserControl
{
    public Card()
    {
        MinHeight = 8;
        MinWidth = 8;
    }

    protected override Type StyleKeyOverride => typeof(Card);

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsCardVisualsEnabledProperty)
        {
            PseudoClasses.Set("disabled", !change.GetNewValue<bool>());
        }
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        PseudoClasses.Set("disabled", !IsCardVisualsEnabled);
    }

    // ReSharper disable MemberCanBePrivate.Global
    public readonly static StyledProperty<bool> IsCardVisualsEnabledProperty =
        AvaloniaProperty.Register<Card, bool>("IsCardVisualsEnabled", true);

    /// <summary>
    ///     Whether to show card visuals.
    ///     When false, the card will have a padding of 0 and be transparent.
    /// </summary>
    public bool IsCardVisualsEnabled
    {
        get => GetValue(IsCardVisualsEnabledProperty);
        set => SetValue(IsCardVisualsEnabledProperty, value);
    }

    // ReSharper restore MemberCanBePrivate.Global
}