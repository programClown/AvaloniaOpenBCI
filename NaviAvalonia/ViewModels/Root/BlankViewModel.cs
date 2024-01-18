using NaviAvalonia.Routing;

namespace NaviAvalonia.ViewModels.Root;

public class BlankViewModel : RoutableScreen, IMainScreenViewModel
{
    /// <inheritdoc />
    public ViewModelBase? TitleBarViewModel => null;
}
