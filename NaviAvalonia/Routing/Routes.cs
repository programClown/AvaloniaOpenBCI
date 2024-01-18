using System.Collections.Generic;
using NaviAvalonia.ViewModels.Root;

namespace NaviAvalonia.Routing;

public static class Routes
{
    public static List<IRouterRegistration> NaviRoutes = new() { new RouteRegistration<BlankViewModel>("blank"), };
}
