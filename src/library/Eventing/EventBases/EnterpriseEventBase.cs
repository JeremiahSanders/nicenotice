using System.Text.Json.Serialization;

namespace Jds.NiceNotice;

/// <summary>
///   A base typed notice supporting unique instance identification (<see cref="Id" />,
///   enabling deduplication) and a timestamp (<see cref="Timestamp" />).
/// </summary>
/// <remarks>
///   <para>
///     A base enterprise event providing a required/core collection of notice properties.
///   </para>
///   <para>
///     This class is not intended to be dispatched directly; it is intended to be extended by a typed notice.
///     However, this type is not abstract because the contents of this type are enough to support communicating
///     that a specific event occurred in certain circumstances.
///   </para>
///   <para>
///     For example, if the event is dispatched to an &quot;application started&quot; I/O channel
///     then the combination of <see cref="Timestamp" /> and <see cref="Id" /> might be sufficient,
///     conveying when the application started and a unique identifier for the occurrence (to support deduplication).
///   </para>
/// </remarks>
public record EnterpriseEventBase
{
  /// <summary>
  ///   Gets the timestamp associated with this enterprise event
  ///   (in general, understood to mean &quot;when&quot; this event occurred).
  /// </summary>
  [JsonPropertyName(name: "timestamp")]
  public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

  /// <summary>
  ///   Gets a unique identifier for this enterprise event.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     In principle, an enterprise event (or any other notice) is a data transfer object; it has no identity.
  ///     However, in practice, enterprise systems often need to identify specific event instances,
  ///     e.g., for deduplication purposes.
  ///   </para>
  /// </remarks>
  [JsonPropertyName(name: "id")]
  public Guid Id { get; init; } = Guid.NewGuid();
}
