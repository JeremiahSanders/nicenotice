namespace Jds.NiceNotice.TypedNotices;

/// <summary>
///   A routed typed notice which is part of a batch.
/// </summary>
/// <remarks>
///   This type is intended to be a response type after batch processing.
///   This type is not used in request parameters as a design decision,
///   preferring to reinforce uniqueness requirements of <paramref name="batchNoticeId" /> within the batch
///   by use of dictionaries.
/// </remarks>
/// <param name="batchNoticeId">The unique identifier for this notice within its batch.</param>
/// <param name="stream">The logical stream to which the notice is dispatched.</param>
/// <param name="notice">The typed notice (<see cref="object" />) which is dispatched.</param>
/// <param name="serializedNotice">
///   The serialized representation of the notice, as returned by the <see cref="INoticeIo" />.
/// </param>
/// <param name="contentType">The content type of the <paramref name="serializedNotice" />, e.g., <c>application/json</c>.</param>
/// <param name="metadata">Optional metadata associated with the notice.</param>
/// <param name="exception">The exception that occurred during processing, if any.</param>
public class BatchRoutedTypedNoticeResponse(
  string batchNoticeId,
  EventStreamId stream,
  object notice,
  string serializedNotice,
  string? contentType,
  IReadOnlyDictionary<string, string>? metadata,
  Exception? exception
) : IoNoticeDispatchResult(stream, serializedNotice, metadata, contentType, exception)
{
  /// <summary>
  ///   Gets the unique identifier for this notice within its batch.
  /// </summary>
  public string BatchNoticeId { get; } = batchNoticeId;

  /// <summary>
  ///   Gets the typed notice (<see cref="object" />) which was dispatched.
  /// </summary>
  public object TypedNotice { get; } = notice;

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

    return Equals((BatchRoutedTypedNoticeResponse)obj);
  }

  /// <inheritdoc />
  public override int GetHashCode()
  {
    return HashCode.Combine(base.GetHashCode(), BatchNoticeId, Notice);
  }

  /// <summary>
  ///   Determines whether the specified object is equal to the current object.
  /// </summary>
  /// <param name="other">The object to compare with the current object.</param>
  /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
  protected bool Equals(BatchRoutedTypedNoticeResponse other)
  {
    return base.Equals(other) && BatchNoticeId == other.BatchNoticeId && Notice.Equals(other.Notice);
  }
}
