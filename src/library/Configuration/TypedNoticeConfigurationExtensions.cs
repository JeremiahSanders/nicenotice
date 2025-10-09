using Microsoft.Extensions.DependencyInjection;

namespace Jds.NiceNotice;

public static class TypedNoticeConfigurationExtensions
{
  public enum RoutingTypes
  {
    TypeFullName,
    TypeName
  }

  public enum SerializationTypes
  {
    Json
  }

  public enum ValidationTypes
  {
    None,
    DataAttributes
  }


  /// <summary>
  ///   This overload
  /// </summary>
  /// <param name="builder"></param>
  /// <param name="configuration"></param>
  /// <param name="lifetime"></param>
  /// <returns></returns>
  public static NiceNoticeBuilder UseTypedNotices(
    this NiceNoticeBuilder builder,
    TypedNoticeConfiguration configuration,
    ServiceLifetime lifetime = ServiceLifetime.Singleton)
  {
    return builder.UseTypedNotices(
      typedNoticesBuilder =>
      {
        switch (configuration.RoutingType)
        {
          case RoutingTypes.TypeFullName:
            typedNoticesBuilder.WithTypeNameStreams(useFullTypeName: true);

            break;
          case RoutingTypes.TypeName:
            typedNoticesBuilder.WithTypeNameStreams(useFullTypeName: false);

            break;
          default:
            typedNoticesBuilder.WithTypeNameStreams(useFullTypeName: false);

            break;
        }

        switch (configuration.SerializationType)
        {
          case SerializationTypes.Json:
            typedNoticesBuilder.WithJsonSerializer();

            break;
          default:
            typedNoticesBuilder.WithJsonSerializer();

            break;
        }

        switch (configuration.ValidationType)
        {
          case ValidationTypes.None:
            typedNoticesBuilder.WithNoValidation();

            break;
          case ValidationTypes.DataAttributes:
            typedNoticesBuilder.WithDataAnnotationsValidator();

            break;
          default:
            typedNoticesBuilder.WithNoValidation();

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
    public RoutingTypes RoutingType { get; set; } = RoutingTypes.TypeName;
    public SerializationTypes SerializationType { get; set; } = SerializationTypes.Json;
    public ValidationTypes ValidationType { get; set; } = ValidationTypes.DataAttributes;
  }
}
