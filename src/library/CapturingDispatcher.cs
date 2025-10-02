using System.Collections.Concurrent;

namespace Jds.NiceNotice;

/// <summary>
///   An implementation of <see cref="INoticeIo" /> which is intended for test purposes.
///   This dispatcher should not be used in a runtime environment; its use can lead to memory leaks.
/// </summary>
public class CapturingDispatcher : INoticeIo
{
  private readonly int _maximumNoticesToRetain;

  public CapturingDispatcher()
    : this(maximumNoticesToRetain: -1)
  {
  }

  private CapturingDispatcher(int maximumNoticesToRetain)
  {
    _maximumNoticesToRetain = maximumNoticesToRetain;
  }

  private ConcurrentQueue<(EventStreamId, string)> Notices { get; } = [];

  /// <summary>
  ///   Gets an enumerator for the captured notices.
  /// </summary>
  public IEnumerable<(EventStreamId, string)> CapturedNotices => Notices;

  /// <inheritdoc />
  public Task<string> DispatchAsync(EventStreamId stream, string notice, CancellationToken cancellationToken = default)
  {
    Notices.Enqueue((stream, notice));

    if (_maximumNoticesToRetain > -1)
    {
      while (Notices.Count > _maximumNoticesToRetain)
      {
        Notices.TryDequeue(out _);
      }
    }

    return Task.FromResult(notice);
  }

  /// <summary>
  ///   Creates a capturing dispatcher which limits the notices it retains.
  /// </summary>
  /// <param name="maximumNoticesToRetain">The maximum count of notices to retain.</param>
  /// <returns>Returns a new <see cref="CapturingDispatcher" />.</returns>
  public static CapturingDispatcher Create(int maximumNoticesToRetain = -1)
  {
    return new CapturingDispatcher(maximumNoticesToRetain);
  }

  /// <summary>
  ///   Purges the notices captured by this instance.
  /// </summary>
  /// <returns>Returns this instance.</returns>
  public CapturingDispatcher PurgeNotices()
  {
    Notices.Clear();

    return this;
  }
}
