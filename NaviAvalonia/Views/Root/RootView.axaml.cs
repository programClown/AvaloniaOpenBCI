using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NaviAvalonia.ViewModels.Root;
using ReactiveUI;

namespace NaviAvalonia.Views.Root;

public partial class RootView : ReactiveUserControl<RootViewModel>
{
    public RootView()
    {
        InitializeComponent();
        // this.WhenActivated(d=>ViewModel.WhenAnyValue(vm=>vm.scre))
    }
}
