using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Jds.NiceNotice.TypedNotices;

namespace Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Custom;

/// <summary>
///   An example, base, typed notice (a.k.a., an enterprise event).
///   This type is not derived from <see cref="EnterpriseEvent" /> so as to show how an organization
///   might define their own base enterprise event type.
/// </summary>
/// <remarks>
///   <para>
///     A base enterprise event represents a required/core collection of notice properties.
///   </para>
///   <para>
///     In this example, an event <see cref="Name" /> and <see cref="Timestamp" /> are required for all
///     derived notices.
///   </para>
///   <para>
///     In other applications, a unique event identifier (possibly implemented as a <see cref="Guid" />)
///     might be necessary.
///     Inclusion of such an identifier could allow downstream processors to deduplicate events
///     or otherwise identify specific notices.
///   </para>
///   <para>
///     Implementers should consider strongly the required/shared properties on a typed notice.
///   </para>
/// </remarks>
public record ExampleCustomBaseEnterpriseEvent
{
  /// <summary>
  ///   Gets the timestamp associated with this enterprise event
  ///   (in general, understood to mean &quot;when&quot; this event occurred).
  /// </summary>
  [JsonPropertyName(name: "ts")]
  public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

  /// <summary>
  ///   Gets the name of this schema/type of enterprise event.
  ///   This is not a message; interpret as an enumeration value shared by all notices of the same &quot;type&quot;.
  /// </summary>
  [Required(AllowEmptyStrings = false)]
  [JsonPropertyName(name: "name")]
  public string Name { get; init; } = string.Empty;
}
