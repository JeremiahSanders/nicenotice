using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.DependencyInjection;

namespace Jds.NiceNotice.Tests.Unit.ServiceArrangementExamples;

public static class ConfigurationOptions
{
  /// <summary>
  ///   <para>
  ///     Test arrangement:
  ///     Adds a default typed notices implementation, where <see cref="EnterpriseEventBase" /> is the base type.
  ///     Requests that:
  ///     notices be serialized to JSON,
  ///     that events be routed to streams based on their full type names,
  ///     and messages are validated using <see cref="Validator" />.
  ///     We register a custom <paramref name="dispatcher"/>.
  ///   </para>
  /// </summary>
  /// <remarks>
  /// </remarks>
  /// <param name="services"></param>
  /// <param name="dispatcher"></param>
  /// <returns></returns>
  public static IServiceCollection ApplyConfigurationObjectConfiguration(
    this IServiceCollection services,
    Func<IServiceProvider, INoticeIo> dispatcher)
  {
    return services
      .AddNiceNotice(builder => builder
        .UseTypedNotices(
          new TypedNoticeConfigurationExtensions.TypedNoticeConfiguration
          {
            RoutingType = TypedNoticeConfigurationExtensions.RoutingTypes.TypeFullName,
            SerializationType = TypedNoticeConfigurationExtensions.SerializationTypes.Json,
            ValidationType = TypedNoticeConfigurationExtensions.ValidationTypes.DataAttributes
          },
          ServiceLifetime.Singleton
        )
        .UseDispatcher(dispatcher, ServiceLifetime.Singleton)
      );
  }
}
