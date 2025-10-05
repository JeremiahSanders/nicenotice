using System.Text.Json.Serialization;

namespace Jds.NiceNotice;

/// <summary>
///   An enterprise event that contains a message.
/// </summary>
public record EnterpriseEventMessage : EnterpriseEventBase
{
  /// <summary>
  ///   Gets the message associated with this enterprise event.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Use caution with the length of values assigned to this property.
  ///   </para>
  ///   <para>
  ///     NiceNotice is expressly designed to simplify sending typed notifications to enterprise event buses
  ///     which have message payload size limits.
  ///     Generic message fields which convey information in human-readable string form
  ///     (rather than discrete data properties)
  ///     can exceed the size limits of the underlying event bus, yielding runtime exceptions.
  ///   </para>
  /// </remarks>
  [JsonPropertyName(name: "message")]
  public required string Message { get; init; } = string.Empty;
}
