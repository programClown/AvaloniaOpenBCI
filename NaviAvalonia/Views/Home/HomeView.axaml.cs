using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NaviAvalonia.ViewModels.Home;

namespace NaviAvalonia.Views.Home;

public partial class HomeView : ReactiveUserControl<HomeViewModel>
{
    public HomeView()
    {
        InitializeComponent();
    }
}
