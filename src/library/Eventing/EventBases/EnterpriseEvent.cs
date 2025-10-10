using System.Text.Json.Serialization;

namespace Jds.NiceNotice;

/// <summary>
///   A base enterprise event (data transfer object),
///   suitable for extending with application-specific properties.
/// </summary>
public abstract record EnterpriseEvent : EnterpriseEventBase
{
  private readonly string _schemaTitle;
  private string? _name;

  /// <summary>
  /// Initializes a new instance of the <see cref="EnterpriseEvent"/> class.
  /// </summary>
  protected EnterpriseEvent()
  {
    _schemaTitle = GetType()
      .Name;
  }

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
  ///   Gets the schema title of this enterprise event.
  /// </summary>
  [JsonPropertyName(name: "schema")]
  public string Schema
  {
    get
    {
      _name ??= CreateSchemaTitle();

      return _name;
    }
    init => _name = value;
  }

  private string CreateSchemaTitle()
  {
    return DefaultSchema(SchemaTitle, SchemaRevision);
  }

  /// <summary>
  ///   Generates an event schema title.
  /// </summary>
  /// <param name="eventTitle"></param>
  /// <param name="eventSchemaRevision"></param>
  /// <returns></returns>
  public static string DefaultSchema(string eventTitle, int? eventSchemaRevision)
  {
    return eventSchemaRevision != null
      ? $"{eventTitle}@{eventSchemaRevision}"
      : eventTitle;
  }
}
