using System.Collections.Concurrent;

namespace Jds.NiceNotice.TypedNotices.Routing.Implementations;

/// <summary>
///   An implementation of <see cref="NoticeRouter{TEnterpriseEventBaseType}" /> that uses a type map
///   (dictionary) to determine the stream ID.
/// </summary>
/// <param name="map">A map of event types to stream IDs.</param>
/// <param name="defaultStream">
///   A default stream which will be returned by <see cref="GetStreamId" /> if a dispatched type
///   isn't configured in <paramref name="map" />.
/// </param>
/// <typeparam name="TEnterpriseEventBaseType">A base enterprise event type.</typeparam>
internal class TypeMapRouter<TEnterpriseEventBaseType>(
  IReadOnlyDictionary<Type, EventStreamId> map,
  EventStreamId? defaultStream = null
)
  : NoticeRouter<TEnterpriseEventBaseType> where TEnterpriseEventBaseType : notnull
{
  /// <summary>
  ///   Gets a private, thread-safe dictionary of event types to stream IDs.
  /// </summary>
  private ConcurrentDictionary<Type, EventStreamId> TypeMap { get; } = new(map);

  /// <inheritdoc />
  /// <exception cref="NoticeRoutingException">
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

      throw new NoticeRoutingException(
        $"No stream defined for event type {noticeType.Name}.",
        innerException: null
      );
    }

    return id;
  }
}
