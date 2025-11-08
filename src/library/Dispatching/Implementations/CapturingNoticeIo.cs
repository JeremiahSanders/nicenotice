using System.Collections.Concurrent;

namespace Jds.NiceNotice.Dispatching.Implementations;

/// <summary>
///   <para>
///     A thread-safe implementation of <see cref="INoticeIo" /> which is intended for test purposes.
///     Each dispatched notice is captured and can be retrieved via <see cref="CapturedNotices" />.
///   </para>
///   <para>
///     This dispatcher should not be used in a runtime environment; its use can lead to memory leaks.
///   </para>
/// </summary>
public class CapturingNoticeIo : INoticeBatchIo
{
  private readonly int _maximumNoticesToRetain;

  /// <summary>
  ///   Initializes a new instance of the <see cref="CapturingNoticeIo" /> class.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     All notices are retained until purged (using <see cref="PurgeNotices" />).
  ///     Use the <see cref="Create" /> method to create an instance which constrains the number of notices retained.
  ///   </para>
  /// </remarks>
  public CapturingNoticeIo()
    : this(maximumNoticesToRetain: -1)
  {
  }

  private CapturingNoticeIo(int maximumNoticesToRetain)
  {
    _maximumNoticesToRetain = maximumNoticesToRetain;
  }

  /// <summary>
  ///   Gets the captured notices.
  /// </summary>
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

  /// <inheritdoc
  ///   cref="INoticeBatchIo.DispatchNoticesAsync(IReadOnlyDictionary{string, BatchedIoRequestNotice}, BatchDispatchOptions, CancellationToken)" />
  public async Task<BatchIoNoticeDispatchResult> DispatchNoticesAsync(
    IReadOnlyDictionary<string, BatchedIoRequestNotice> notices,
    BatchDispatchOptions? batchDispatchOptions = null,
    CancellationToken cancellationToken = default)
  {
    ParallelOptions parallelOptions = new()
    {
      MaxDegreeOfParallelism = batchDispatchOptions?.MaxDegreeOfParallelism ?? 1,
      CancellationToken = cancellationToken
    };
    ConcurrentBag<BatchedIoResponseNotice> successes = [];
    ConcurrentBag<(BatchedIoResponseNotice, Exception)> failures = [];
    await Parallel.ForEachAsync(
      notices
        .Select(static notice => new BatchedIoResponseNotice(notice.Key, notice.Value.Stream, notice.Value.Notice)),
      parallelOptions,
      async (notice, token) =>
      {
        try
        {
          await DispatchAsync(notice.Stream, notice.Notice, token);
          successes.Add(notice);
        }
        catch (Exception e)
        {
          failures.Add((notice, e));
        }
      }
    );

    return new BatchIoNoticeDispatchResult
    {
      Failures = failures.ToList(),
      Successes = successes.ToList()
    };
  }

  /// <summary>
  ///   Creates a capturing dispatcher which limits the notices it retains.
  /// </summary>
  /// <param name="maximumNoticesToRetain">The maximum count of notices to retain.</param>
  /// <returns>Returns a new <see cref="CapturingNoticeIo" />.</returns>
  public static CapturingNoticeIo Create(int maximumNoticesToRetain = -1)
  {
    return new CapturingNoticeIo(maximumNoticesToRetain);
  }

  /// <summary>
  ///   Purges the notices captured by this instance.
  /// </summary>
  /// <returns>Returns this instance.</returns>
  public CapturingNoticeIo PurgeNotices()
  {
    Notices.Clear();

    return this;
  }
}
