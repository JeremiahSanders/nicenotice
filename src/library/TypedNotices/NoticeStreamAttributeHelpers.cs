using System.Collections.Concurrent;
using System.Reflection;

namespace Jds.NiceNotice.TypedNotices;

/// <summary>
///   Methods which support extracting event stream identifiers from <see cref="NoticeStreamAttribute" />.
/// </summary>
internal static class NoticeStreamAttributeHelpers
{
  /// <summary>
  ///   A cache of event types to their corresponding event stream ids.
  /// </summary>
  private static readonly ConcurrentDictionary<Type, EventStreamId> TypeStreamIdCache = new();

  /// <summary>
  ///   A cache of event types for which the stream name is known to be undefined.
  /// </summary>
  /// <remarks>
  ///   Used to avoid repeated reflection lookups for event types for which the stream name is
  ///   known to be undefined.
  /// </remarks>
  private static readonly ConcurrentBag<Type> TypeStreamIdKnownUndefined = [];

  public static EventStreamId? TryGetNoticeEventStreamId(Type eventType)
  {
    if (TypeStreamIdCache.TryGetValue(eventType, out EventStreamId streamId))
    {
      return streamId;
    }

    string? streamName = TryGetNoticeStreamName(eventType);

    if (streamName is null)
    {
      TypeStreamIdKnownUndefined.Add(eventType);

      return null;
    }

    EventStreamId id = EventStreamId.From(streamName);

    TypeStreamIdCache[eventType] = id;

    return id;
  }

  public static EventStreamId? TryGetNoticeEventStreamId<TEvent>(TEvent @event)
  {
    return @event is null ? null : TryGetNoticeEventStreamId(@event.GetType());
  }

  private static string? TryGetNoticeStreamName(Type eventType)
  {
    try
    {
      if (TypeStreamIdKnownUndefined.Contains(eventType))
      {
        return null;
      }

      return eventType
        .GetCustomAttribute<NoticeStreamAttribute>(inherit: true)
        ?.StreamName;
    }
    catch (Exception _)
    {
      return null;
    }
  }
}
