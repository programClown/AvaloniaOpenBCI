using System;
using FluentAvalonia.UI.Controls;

namespace AvaloniaOpenBCI.Controls;

/// <summary>
///     Like <see cref="HyperlinkButton" />, but with a link icon left of the text content.
/// </summary>
public class HyperlinkIconButton : HyperlinkButton
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(HyperlinkIconButton);
}