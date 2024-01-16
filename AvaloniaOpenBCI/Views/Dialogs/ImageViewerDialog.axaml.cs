using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AvaloniaOpenBCI.Controls;

namespace AvaloniaOpenBCI.Views.Dialogs;

public partial class ImageViewerDialog : UserControlBase
{
    public static readonly StyledProperty<bool> IsFooterEnabledProperty = AvaloniaProperty.Register<
        ImageViewerDialog,
        bool
    >("IsFooterEnabled");

    /// <summary>
    /// Whether the footer with file name / size will be shown.
    /// </summary>
    public bool IsFooterEnabled
    {
        get => GetValue(IsFooterEnabledProperty);
        set => SetValue(IsFooterEnabledProperty, value);
    }

    public ImageViewerDialog()
    {
        InitializeComponent();
    }

    private void InfoButton_OnTapped(object? sender, TappedEventArgs e)
    {
        InfoTeachingTip.IsOpen ^= true;
    }
}
