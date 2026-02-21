using System.ComponentModel.DataAnnotations;

namespace Jds.NiceNotice.Configuration;

/// <summary>
///   Configuration-based setup parameters for typed notices.
/// </summary>
/// <remarks>
///   This type supports <see cref="TypedNoticesNiceNoticeBuilderExtensions.UseTypedNotices" />.
/// </remarks>
public class TypedNoticesBuilderOptions
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
  ///   Gets or sets the routing type.
  /// </summary>
  public RoutingTypes RoutingType { get; init; } = RoutingTypes.TypeName;

  /// <summary>
  ///   Gets or sets the serialization type.
  /// </summary>
  public SerializationTypes SerializationType { get; init; } = SerializationTypes.Json;

  /// <summary>
  ///   Gets or sets the validation type.
  /// </summary>
  public ValidationTypes ValidationType { get; init; } = ValidationTypes.DataAttributes;
}
