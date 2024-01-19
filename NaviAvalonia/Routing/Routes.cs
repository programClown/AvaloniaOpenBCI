using System.Collections.Generic;
using NaviAvalonia.ViewModels.Home;
using NaviAvalonia.ViewModels.Root;
using NaviAvalonia.ViewModels.Settings;

namespace NaviAvalonia.Routing;

public static class Routes
{
    public static List<IRouterRegistration> NaviRoutes =
        new()
        {
            new RouteRegistration<BlankViewModel>("blank"),
            new RouteRegistration<HomeViewModel>("home"),
            new RouteRegistration<SettingsViewModel>("settings"),
        };
}
