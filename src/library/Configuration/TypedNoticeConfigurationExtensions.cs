using System.ComponentModel.DataAnnotations;

using Jds.NiceNotice.Dispatching;
using Jds.NiceNotice.TypedNotices;
using Jds.NiceNotice.TypedNotices.Routing;
using Jds.NiceNotice.TypedNotices.Serialization;
using Jds.NiceNotice.TypedNotices.Validation;

using Microsoft.Extensions.DependencyInjection;

namespace Jds.NiceNotice.Configuration;

/// <summary>
///   Methods extending the NiceNoticeBuilder to support configuration-based setup for typed notices.
/// </summary>
public static class TypedNoticeConfigurationExtensions
{
  /// <summary>
  ///   Notice event stream routing algorithms which are supported for configuration-based setup.
  /// </summary>
  public enum RoutingTypes
  {
    /// <summary>
    ///   A routing implementation using the full type name (including namespace).
    /// </summary>
    TypeFullName,

    /// <summary>
    ///   A routing implementation using the type name only.
    /// </summary>
    TypeName
  }

  /// <summary>
  ///   Notice serialization algorithms which are supported for configuration-based setup.
  /// </summary>
  public enum SerializationTypes
  {
    /// <summary>
    ///   A serialization implementation using JSON.
    /// </summary>
    Json
  }

  /// <summary>
  ///   Notice validation algorithms which are supported for configuration-based setup.
  /// </summary>
  public enum ValidationTypes
  {
    /// <summary>
    ///   No validation.
    /// </summary>
    None,

    /// <summary>
    ///   Validation using data annotations data attributes, e.g., <see cref="RequiredAttribute" />.
    /// </summary>
    DataAttributes
  }

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
    TypedNoticeConfiguration configuration,
    ServiceLifetime lifetime = ServiceLifetime.Scoped)
  {
    return builder.UseTypedNotices(
      typedNoticesBuilder =>
      {
        switch (configuration.RoutingType)
        {
          case RoutingTypes.TypeFullName:
            typedNoticesBuilder.RouteToTypeNameStreams(useFullTypeName: true);

            break;
          case RoutingTypes.TypeName:
            typedNoticesBuilder.RouteToTypeNameStreams(useFullTypeName: false);

            break;
          default:
            typedNoticesBuilder.RouteToTypeNameStreams(useFullTypeName: false);

            break;
        }

        switch (configuration.SerializationType)
        {
          case SerializationTypes.Json:
            typedNoticesBuilder.SerializeToJson();

            break;
          default:
            typedNoticesBuilder.SerializeToJson();

            break;
        }

        switch (configuration.ValidationType)
        {
          case ValidationTypes.None:
            typedNoticesBuilder.ValidateNothing();

            break;
          case ValidationTypes.DataAttributes:
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

  /// <summary>
  ///   Configuration-based setup for typed notices.
  /// </summary>
  public class TypedNoticeConfiguration
  {
    /// <summary>
    ///   Gets or sets the routing type.
    /// </summary>
    public RoutingTypes RoutingType { get; set; } = RoutingTypes.TypeName;

    /// <summary>
    ///   Gets or sets the serialization type.
    /// </summary>
    public SerializationTypes SerializationType { get; set; } = SerializationTypes.Json;

    /// <summary>
    ///   Gets or sets the validation type.
    /// </summary>
    public ValidationTypes ValidationType { get; set; } = ValidationTypes.DataAttributes;
  }
}
