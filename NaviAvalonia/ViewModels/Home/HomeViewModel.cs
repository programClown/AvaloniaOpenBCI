using NaviAvalonia.Routing;

namespace NaviAvalonia.ViewModels.Home;

public class HomeViewModel : RoutableScreen, IMainScreenViewModel
{
    public HomeViewModel() { }

    public ViewModelBase? TitleBarViewModel => null;
}
