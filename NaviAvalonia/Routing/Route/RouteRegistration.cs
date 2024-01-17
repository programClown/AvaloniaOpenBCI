using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace NaviAvalonia.Routing;

public class RouteRegistration<TViewModel> : IRouterRegistration
    where TViewModel : RoutableScreen
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RouteRegistration{TViewModel}" /> class.
    /// </summary>
    /// <param name="path">The path of the route.</param>
    public RouteRegistration(string path)
    {
        Route = new Route(path);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{nameof(Route)}: {Route}, {nameof(ViewModel)}: {ViewModel}";
    }

    /// <summary>
    ///     Gets the route associated with this registration.
    /// </summary>
    public Route Route { get; }

    /// <inheritdoc />
    public Type ViewModel => typeof(TViewModel);

    /// <inheritdoc />
    public List<IRouterRegistration> Children { get; set; } = new();
}
