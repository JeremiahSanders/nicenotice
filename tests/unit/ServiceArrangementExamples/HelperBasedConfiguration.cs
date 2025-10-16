using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.DependencyInjection;

namespace Jds.NiceNotice.Tests.Unit.ServiceArrangementExamples;

public static class HelperBasedConfiguration
{
  /// <summary>
  ///   <para>
  ///     Test arrangement:
  ///     Adds a default typed notices implementation, where <see cref="EnterpriseEvent" /> is the base type.
  ///     Requests that:
  ///     notices be serialized to JSON,
  ///     that events be routed to streams based on their type names,
  ///     and messages are validated using <see cref="Validator" />.
  ///     We register a custom dispatcher which sends notices to xUnit's test output.
  ///   </para>
  /// </summary>
  /// <remarks>
  /// </remarks>
  /// <param name="services"></param>
  /// <param name="dispatcher"></param>
  /// <returns></returns>
  public static IServiceCollection ApplyHelperBasedConfiguration(
    this IServiceCollection services,
    Func<IServiceProvider, INoticeIo> dispatcher)
  {
    return services
      .AddNiceNotice(builder => builder
        .UseTypedNotices(
          eeBuilder => eeBuilder
            .SerializeToJson()
            .RouteToTypeNameStreams()
            .ValidateWithDataAnnotations(),
          ServiceLifetime.Singleton
        )
        .UseDispatcher(dispatcher, ServiceLifetime.Singleton)
      );
  }
}
