using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;

// ReSharper disable once CheckNamespace
namespace NaviAvalonia.Routing;

/// <summary>
///     For internal use.
/// </summary>
internal interface IRoutableScreen : IActivatableViewModel
{
    Task InternalOnNavigating(NavigationArguments args, CancellationToken cancellationToken);
    Task InternalOnClosing(NavigationArguments args);
}
