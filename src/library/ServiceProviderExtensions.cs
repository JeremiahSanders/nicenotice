using Jds.NiceNotice.Configuration;

using Microsoft.Extensions.DependencyInjection;

namespace Jds.NiceNotice;

/// <summary>
///   Methods extending <see cref="IServiceProvider" />.
/// </summary>
internal static class ServiceProviderExtensions
{
  internal static TService GetServiceOrThrowMissingDependency<TService>(this IServiceProvider provider)
    where TService : class
  {
    return MissingDependencyException.ThrowIfNull(
      provider.GetService<TService>()
    );
  }
}
