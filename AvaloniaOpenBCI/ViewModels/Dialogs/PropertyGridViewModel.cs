using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.PropertyGrid.ViewModels;
using AvaloniaOpenBCI.Attributes;
using AvaloniaOpenBCI.Controls;
using AvaloniaOpenBCI.ViewModels.Base;
using AvaloniaOpenBCI.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using OneOf;

namespace AvaloniaOpenBCI.ViewModels.Dialogs;

[View(typeof(PropertyGridDialog))]
public partial class PropertyGridViewModel : ContentDialogViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedObjectItemsSource))]
    private OneOf<INotifyPropertyChanged, IEnumerable<INotifyPropertyChanged>>? selectedObject;

    public IEnumerable<INotifyPropertyChanged>? SelectedObjectItemsSource =>
        SelectedObject?.Match(single => [single], multiple => multiple);

    [ObservableProperty]
    private PropertyGridShowStyle _showStyle = PropertyGridShowStyle.Alphabetic;

    [ObservableProperty]
    private IReadOnlyList<string>? _excludeCategories;

    [ObservableProperty]
    private IReadOnlyList<string>? _includeCategories;

    /// <inheritdoc />
    public override BetterContentDialog GetDialog()
    {
        var dialog = base.GetDialog();

        dialog.Padding = new Thickness(0);
        dialog.CloseOnClickOutside = true;
        dialog.CloseButtonText = "关闭";

        return dialog;
    }
}
