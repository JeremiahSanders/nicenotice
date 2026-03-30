namespace Jds.NiceNotice;

/// <summary>
///   A notice I/O request, which may be part of a batch.
/// </summary>
/// <param name="stream">A logical event stream to which the notice is routed.</param>
/// <param name="notice">The content of the notice which is being dispatched.</param>
/// <param name="metadata">Optional. Metadata associated with the notice.</param>
/// <param name="contentType">
///   Optional. The content type of the notice,
///   e.g., <c>application/json</c> or <c>text/plain</c>.
/// </param>
public class IoNoticeDispatchRequest(
  EventStreamId stream,
  string notice,
  IReadOnlyDictionary<string, string>? metadata,
  string? contentType
)
{
  /// <summary>
  ///   Gets the content type of the notice (e.g., <c>application/json</c>).
  /// </summary>
  public string? ContentType { get; init; } = contentType;

  /// <summary>
  ///   Gets the metadata associated with the notice.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Some I/O implementations use this data to provide additional metadata about the notice,
  ///     enabling additional features for applications subscribed to the notice stream.
  ///     For example, AWS SNS supports <c>MessageAttributes</c>
  ///     which can be used to store metadata which enables topic subscription filtering.
  ///   </para>
  /// </remarks>
  public IReadOnlyDictionary<string, string>? Metadata { get; init; } = metadata;

  /// <summary>
  ///   Gets the content of the notice which is being dispatched.
  /// </summary>
  public string Notice { get; } = notice;

  /// <summary>
  ///   Gets the logical event stream to which the notice is routed.
  /// </summary>
  public EventStreamId Stream { get; } = stream;

  /// <summary>
  ///   Creates a new instance of <see cref="IoNoticeDispatchRequest" />.
  /// </summary>
  /// <param name="stream">A destination stream.</param>
  /// <param name="notice">A notice (possibly serialized).</param>
  /// <param name="metadata">Metadata related to <paramref name="notice" />.</param>
  /// <param name="contentType">A content type for the <paramref name="notice" />.</param>
  /// <returns>Returns the created <see cref="IoNoticeDispatchRequest" />.</returns>
  public static IoNoticeDispatchRequest Create(
    EventStreamId stream,
    string notice,
    IReadOnlyDictionary<string, string>? metadata = null,
    string? contentType = null)
  {
    return new IoNoticeDispatchRequest(stream, notice, metadata, contentType);
  }

  /// <inheritdoc />
  public override bool Equals(object? obj)
  {
    if (obj is null)
    {
      return false;
    }

    if (ReferenceEquals(this, obj))
    {
      return true;
    }

    if (obj.GetType() != GetType())
    {
      return false;
    }

    return Equals((IoNoticeDispatchRequest)obj);
  }

  /// <inheritdoc />
  public override int GetHashCode()
  {
    return HashCode.Combine(ContentType, Metadata, Notice, Stream);
  }

  /// <summary>
  ///   Determines whether the specified object is equal to the current object.
  /// </summary>
  /// <param name="other">The object to compare with the current object.</param>
  /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
  protected bool Equals(IoNoticeDispatchRequest other)
  {
    return ContentType == other.ContentType
           && Notice == other.Notice
           && Stream.Equals(other.Stream)
           && (Equals(Metadata, other.Metadata)
               || (
                 Metadata != null &&
                 other.Metadata != null &&
                 Metadata.Keys.SequenceEqual(other.Metadata.Keys) &&
                 Metadata.Values.SequenceEqual(other.Metadata.Values)
               )
           );
  }
}
