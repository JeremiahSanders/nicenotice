using Jds.NiceNotice.TypedNotices.Routing;
using Jds.NiceNotice.TypedNotices.Serialization;
using Jds.NiceNotice.TypedNotices.Validation;

using Microsoft.Extensions.DependencyInjection;

namespace Jds.NiceNotice.Configuration;

/// <summary>
///   Methods extending the NiceNoticeBuilder to support configuration-based setup for typed notices.
/// </summary>
public static class TypedNoticesNiceNoticeBuilderExtensions
{
  /// <summary>
  ///   Adds support for dispatching typed, serialized notices (JSON most commonly)
  ///   using the default <see cref="EnterpriseEvent" /> as the assumed base type.
  /// </summary>
  /// <remarks>
  ///   Use the
  ///   <see
  ///     cref="NiceNoticeBuilder.UseTypedNotices{TNoticeBaseType}(System.Action{Jds.NiceNotice.Configuration.TypedNoticesBuilder{TNoticeBaseType}},Microsoft.Extensions.DependencyInjection.ServiceLifetime)" />
  ///   overload to specify a different base type.
  /// </remarks>
  /// <param name="builder">This nice notice builder instance.</param>
  /// <param name="configuration">
  ///   A configuration object, used to configure typed notice dispatching using predefined algorithms.
  /// </param>
  /// <param name="lifetime">
  ///   A service lifetime for the typed notice dispatching services.
  ///   The typed notice dispatcher depends upon the configured <see cref="INoticeIo" />,
  ///   so be considerate of the thread-safety and best practices of your I/O implementation.
  /// </param>
  /// <returns></returns>
  public static NiceNoticeBuilder UseTypedNotices(
    this NiceNoticeBuilder builder,
    TypedNoticesBuilderOptions configuration,
    ServiceLifetime lifetime = ServiceLifetime.Scoped)
  {
    return builder.UseTypedNotices(
      typedNoticesBuilder =>
      {
        switch (configuration.RoutingType)
        {
          case TypedNoticesBuilderOptions.RoutingTypes.TypeFullName:
            typedNoticesBuilder.RouteToTypeNameStreams(useFullTypeName: true);

            break;
          case TypedNoticesBuilderOptions.RoutingTypes.TypeName:
            typedNoticesBuilder.RouteToTypeNameStreams(useFullTypeName: false);

            break;
          default:
            typedNoticesBuilder.RouteToTypeNameStreams(useFullTypeName: false);

            break;
        }

        switch (configuration.SerializationType)
        {
          case TypedNoticesBuilderOptions.SerializationTypes.Json:
            typedNoticesBuilder.SerializeToJson();

            break;
          default:
            typedNoticesBuilder.SerializeToJson();

            break;
        }

        switch (configuration.ValidationType)
        {
          case TypedNoticesBuilderOptions.ValidationTypes.None:
            typedNoticesBuilder.ValidateNothing();

            break;
          case TypedNoticesBuilderOptions.ValidationTypes.DataAttributes:
            typedNoticesBuilder.ValidateWithDataAnnotations();

            break;
          default:
            typedNoticesBuilder.ValidateNothing();

            break;
        }
      },
      lifetime
    );
  }
}
