using System.Collections.Concurrent;

namespace Jds.NiceNotice;

internal class TypeMapStreamSelector<TEnterpriseEventBaseType>(
  IReadOnlyDictionary<Type, EventStreamId> map,
  EventStreamId? defaultStream = null
)
  : NoticeStreamSelector<TEnterpriseEventBaseType> where TEnterpriseEventBaseType : notnull
{
  /// <summary>
  ///   Gets a private, thread-safe dictionary of event types to stream IDs.
  /// </summary>
  private ConcurrentDictionary<Type, EventStreamId> TypeMap { get; } = new(map);

  /// <inheritdoc />
  /// <exception cref="StreamDeterminationException">
  ///   Thrown when the event type is not configured
  ///   and no default stream is set.
  /// </exception>
  public override EventStreamId GetStreamId<TEventType>(TEventType notice)
  {
    Type noticeType = notice.GetType();

    if (!TypeMap.TryGetValue(noticeType, out EventStreamId id))
    {
      if (defaultStream != null)
      {
        return defaultStream.Value;
      }

      throw new StreamDeterminationException(
        $"No stream defined for event type {noticeType.Name}.",
        innerException: null
      );
    }

    return id;
  }
}
