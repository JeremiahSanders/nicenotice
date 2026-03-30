namespace Jds.NiceNotice.Dispatching;

/// <summary>
///   A routed notice that is part of a batch I/O response.
/// </summary>
/// <param name="batchNoticeId">An identifier for this notice within the batch.</param>
/// <param name="stream">A logical notification stream identifier.</param>
/// <param name="notice">The content of the notification message.</param>
/// <param name="metadata">Optional metadata associated with the notice.</param>
/// <param name="contentType">The content type of the notice, e.g., <c>text/plain</c>.</param>
/// <param name="exception">
///   Any exception that occurred during the dispatch.
///   If null, the dispatch is considered successful.
/// </param>
public class IoBatchNoticeDispatchResultItem(
  string batchNoticeId,
  EventStreamId stream,
  string notice,
  IReadOnlyDictionary<string, string>? metadata,
  string? contentType,
  Exception? exception
)
  : IoNoticeDispatchResult(stream, notice, metadata, contentType, exception)
{
  /// <summary>
  ///   Gets the identifier for this notice within its batch.
  /// </summary>
  public string BatchNoticeId { get; } = batchNoticeId;

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

    return Equals((IoBatchNoticeDispatchResultItem)obj);
  }

  /// <inheritdoc />
  public override int GetHashCode()
  {
    return HashCode.Combine(base.GetHashCode(), BatchNoticeId);
  }

  /// <summary>
  ///   Determines whether the specified <see cref="IoBatchNoticeDispatchResultItem" /> is equal to the current
  ///   <see cref="IoBatchNoticeDispatchResultItem" />.
  /// </summary>
  /// <param name="other">Another <see cref="IoBatchNoticeDispatchResultItem" /> to compare with this instance.</param>
  /// <returns>Returns <c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
  protected bool Equals(IoBatchNoticeDispatchResultItem other)
  {
    return base.Equals(other) && BatchNoticeId == other.BatchNoticeId;
  }
}
