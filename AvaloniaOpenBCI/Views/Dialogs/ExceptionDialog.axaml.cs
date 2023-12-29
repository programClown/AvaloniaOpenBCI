using Avalonia.Interactivity;
using AvaloniaOpenBCI.Attributes;
using AvaloniaOpenBCI.Controls;
using FluentAvalonia.UI.Windowing;

namespace AvaloniaOpenBCI.Views.Dialogs;

[Transient]
public partial class ExceptionDialog : AppWindowBase
{
    public ExceptionDialog()
    {
        InitializeComponent();

        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
    }

    private void ExitButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}