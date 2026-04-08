using Microsoft.Extensions.DependencyInjection;

namespace Jds.NiceNotice.Tests.Unit.ServiceArrangementExamples;

public static class MinimumConfiguration
{
  /// <summary>
  ///   Applies the minimum possible NiceNotice configuration (none).
  ///   NiceNotice should not dispatch any notices.
  ///   However, all the NiceNotice infrastructure will still be registered.
  ///   It is expected that all the normal services will resolve and that <see cref="EnterpriseEvent" />
  ///   will be available as a typed notification base.
  /// </summary>
  /// <param name="services">This service collection.</param>
  /// <returns>Returns this service collection.</returns>
  public static IServiceCollection ApplyMinimumConfiguration(
    this IServiceCollection services
  )
  {
    return services
      .AddNiceNotice(static _ => { });
  }
}
