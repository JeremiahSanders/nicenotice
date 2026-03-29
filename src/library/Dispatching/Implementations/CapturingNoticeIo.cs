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
    RequestNotices = new BoundedConcurrentQueue<IoRequestNotice>(maximumNoticesToRetain);
  }

  /// <summary>
  ///   Gets an enumerator for the captured request notices.
  /// </summary>
  public IEnumerable<IoRequestNotice> CapturedNotices => RequestNotices.Items;

  /// <summary>
  ///   Gets the captured request notices.
  /// </summary>
  private BoundedConcurrentQueue<IoRequestNotice> RequestNotices { get; }

  /// <inheritdoc />
  public Task<IoNoticeDispatchResult> DispatchAsync(
    IoRequestNotice notice,
    CancellationToken cancellationToken = default)
  {
    RequestNotices.Enqueue(notice);

    return Task.FromResult(
      new IoNoticeDispatchResult(notice.Stream, notice.Notice, notice.Metadata, notice.ContentType, exception: null)
    );
  }

  /// <inheritdoc
  ///   cref="INoticeBatchIo.DispatchNoticesAsync" />
  public async Task<BatchIoNoticeDispatchResult> DispatchNoticesAsync(
    BatchIoRequest request,
    CancellationToken cancellationToken = default)
  {
    ParallelOptions parallelOptions = new()
    {
      MaxDegreeOfParallelism = request.BatchDispatchOptions?.MaxDegreeOfParallelism ?? 1,
      CancellationToken = cancellationToken
    };
    ConcurrentBag<BatchedIoResponseNotice> successes = [];
    ConcurrentBag<BatchedIoResponseNotice> failures = [];
    await Parallel.ForEachAsync(
      request.Notices
        .Select(static notice => new BatchedIoResponseNotice(
            notice.Key,
            notice.Value.Stream,
            notice.Value.Notice,
            notice.Value.Metadata,
            notice.Value.ContentType,
            exception: null
          )
        ),
      parallelOptions,
      async (notice, token) =>
      {
        try
        {
          await DispatchAsync(
            new IoRequestNotice(notice.Stream, notice.Notice, notice.Metadata, notice.ContentType),
            token
          );
          successes.Add(notice);
        }
        catch (Exception e)
        {
          failures.Add(
            new BatchedIoResponseNotice(
              notice.BatchNoticeId,
              notice.Stream,
              notice.Notice,
              notice.Metadata,
              notice.ContentType,
              e
            )
          );
        }
      }
    );

    return new BatchIoNoticeDispatchResult(failures.Concat(successes));
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
    RequestNotices.Clear();

    return this;
  }

  /// <summary>
  ///   A wrapper around a <see cref="ConcurrentQueue{T}" /> which enforces a maximum number of items.
  /// </summary>
  /// <param name="maximumItemsToRetain">A maximum number of items to retain.</param>
  /// <typeparam name="T">A queue item type.</typeparam>
  private sealed class BoundedConcurrentQueue<T>(int maximumItemsToRetain)
  {
    private readonly ConcurrentQueue<T> _queue = [];
    private int _count;

    /// <summary>
    ///   Gets an enumerator for the items in the queue.
    /// </summary>
    public IEnumerable<T> Items => _queue;

    /// <summary>
    ///   Removes all items from the queue.
    /// </summary>
    public void Clear()
    {
      while (_queue.TryDequeue(out _))
      {
        Interlocked.Decrement(ref _count);
      }
    }

    /// <summary>
    ///   Enqueue the item.
    /// </summary>
    /// <param name="item">The item to be enqueued.</param>
    public void Enqueue(T item)
    {
      // No work needed if we want no items retained.
      if (maximumItemsToRetain == 0)
      {
        return;
      }

      // Enqueue the item.
      _queue.Enqueue(item);

      // If the maximum is negative, we're treating that as "no limit".
      if (maximumItemsToRetain < 0)
      {
        return;
      }

      // Once we get here, we know we have to enforce a maximum number of items (so we have to track the count
      //   and remove any excess items).
      int currentCount = Interlocked.Increment(ref _count);

      while (currentCount > maximumItemsToRetain && _queue.TryDequeue(out _))
      {
        currentCount = Interlocked.Decrement(ref _count);
      }
    }
  }
}
