namespace Jds.NiceNotice.TypedNotices.Metadata;

/// <summary>
///   An abstraction representing the algorithm used for extracting metadata from a notice.
/// </summary>
/// <typeparam name="TEnterpriseEventBaseType">A base type for enterprise events.</typeparam>
public abstract class NoticeMetadataProvider<TEnterpriseEventBaseType>
  where TEnterpriseEventBaseType : notnull
{
  /// <summary>
  ///   Gets the metadata for the provided notice.
  /// </summary>
  /// <param name="notice">The notice being dispatched.</param>
  /// <param name="serializedNotice">The serialized representation of the notice.</param>
  /// <param name="serializedContentType">The content type of the serialized representation of the notice.</param>
  /// <typeparam name="TEventType">The event type, constrained to <typeparamref name="TEnterpriseEventBaseType" />.</typeparam>
  /// <returns>Returns the metadata for the provided notice.</returns>
  public abstract IReadOnlyDictionary<string, string>? GetMetadata<TEventType>(
    TEventType notice,
    string serializedNotice,
    string? serializedContentType
  )
    where TEventType : TEnterpriseEventBaseType;
}

/// <summary>
///   An abstraction representing the algorithm used for extracting metadata from a notice.
/// </summary>
public abstract class NoticeMetadataProvider
{
  /// <summary>
  ///   Gets the metadata for the provided notice.
  /// </summary>
  /// <param name="notice">The notice being dispatched.</param>
  /// <param name="serializedNotice">The serialized representation of the notice.</param>
  /// <param name="serializedContentType">The content type of the serialized representation of the notice.</param>
  /// <typeparam name="TEventType">The event type.</typeparam>
  /// <returns>Returns the metadata for the provided notice.</returns>
  public abstract IReadOnlyDictionary<string, string>? GetMetadata<TEventType>(
    TEventType notice,
    string serializedNotice,
    string? serializedContentType
  )
    where TEventType : notnull;
}
