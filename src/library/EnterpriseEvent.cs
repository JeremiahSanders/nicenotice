using System.Text.Json.Serialization;

namespace Jds.NiceNotice;

/// <summary>
///   <para>
///     A base enterprise event (data transfer object),
///     suitable for extending with application-specific properties.
///   </para>
///   <para>
///     This type supports:
///     <list type="bullet">
///       <item>
///         unique instance identification (<see cref="Id" />, enabling deduplication),
///       </item>
///       <item>
///         a timestamp (<see cref="Timestamp" />),
///       </item>
///       <item>
///         and schema identification (<see cref="Schema" />, supporting external filtering/logic).
///       </item>
///     </list>
///   </para>
/// </summary>
/// <remarks>
///   <para>
///     Conceptually, a base enterprise event provides a required/core collection of notice properties.
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
public record EnterpriseEvent : INoticeMetadata
{
  private readonly string _schemaTitle;
  private string? _schema;

  /// <summary>
  ///   Initializes a new instance of the <see cref="EnterpriseEvent" /> class.
  /// </summary>
  public EnterpriseEvent()
  {
    _schemaTitle = GetType()
      .Name;
  }

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

  /// <summary>
  ///   Gets the schema identifier of this enterprise event. Default: The type name.
  /// </summary>
  /// <remarks>
  ///   <para>This property is intended to support JSON Schema. This declares the schema to which this event conforms.</para>
  ///   <para>
  ///     Optimally, this will be set to a resolvable URL which provides a JSON Schema document.
  ///     Such configuration supports developer and diagnostic tools, e.g., IDEs.
  ///   </para>
  ///   <para>
  ///     <a href="https://json-schema.org/understanding-json-schema/keywords#dollarschema">
  ///       See JSON Schema documentation for <c>$schema</c>.
  ///     </a>
  ///   </para>
  /// </remarks>
  [JsonPropertyName(name: "$schema")]
  public string Schema
  {
    get
    {
      _schema ??= DefaultSchema(SchemaTitle, SchemaRevision);

      return _schema;
    }
    init => _schema = value;
  }

  /// <summary>
  ///   Gets the timestamp associated with this enterprise event
  ///   (in general, understood to mean &quot;when&quot; this event occurred).
  /// </summary>
  [JsonPropertyName(name: "timestamp")]
  public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

  /// <summary>
  ///   Gets the revision index of this enterprise event data transfer object schema.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     This property is used to provide schema revision/change information, with the assumed initial revision being 0.
  ///     When not null this value is, by default, incorporated into <see cref="Schema" />.
  ///   </para>
  ///   <para>
  ///     The default <see cref="Schema" /> pattern, <c>Title@Revision</c> (e.g., <c>Logout@3</c>, allows applications
  ///     to communicate and document an expectation.
  ///   </para>
  /// </remarks>
  [JsonIgnore]
  protected virtual int? SchemaRevision { get; init; }

  /// <summary>
  ///   Gets the name of this enterprise event data transfer object schema, defaulting to the type name.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     This property is intended to support event naming which is decoupled from the type name.
  ///   </para>
  ///   <para>
  ///     Notifications dispatched across an event bus are intended for use by other applications.
  ///     A clear, documented name for events supports other applications' use
  ///     by providing guidance for the event's schema.
  ///     This is especially important when capturing and storing event notifications,
  ///     as they are often interpreted at a later date.
  ///   </para>
  /// </remarks>
  [JsonIgnore]
  protected virtual string SchemaTitle
  {
    get => _schemaTitle;
    init => _schemaTitle = value;
  }

  /// <inheritdoc />
  /// <remarks>
  ///   <para>Override this method to provide metadata for the event when it is dispatched to I/O.</para>
  /// </remarks>
  public virtual IReadOnlyDictionary<string, string>? GetMetadata()
  {
    return null;
  }

  /// <summary>
  ///   Generates an event schema from the provided event schema title and an optional schema revision index.
  ///   Pattern:
  ///   If the schema is provided, <c>Title@Revision</c>.
  ///   Otherwise, <paramref name="eventTitle" /> is returned unchanged.
  /// </summary>
  /// <param name="eventTitle">An event schema title.</param>
  /// <param name="eventSchemaRevision">
  ///   Optional. A schema revision index.
  ///   (Initial schema: 0, first revision: 1, etc.)
  /// </param>
  /// <returns></returns>
  public static string DefaultSchema(string eventTitle, int? eventSchemaRevision)
  {
    return eventSchemaRevision != null
      ? $"{eventTitle}@{eventSchemaRevision}"
      : eventTitle;
  }
}
