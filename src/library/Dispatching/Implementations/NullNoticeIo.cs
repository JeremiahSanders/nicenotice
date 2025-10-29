namespace Jds.NiceNotice;

/// <summary>
///   Provides a no-operation implementation of <see cref="INoticeIo" />,
///   primarily used as a default or placeholder where event dispatching is not required.
/// </summary>
public class NullNoticeIo : INoticeIo, INoticeBatchIo
{
  /// <inheritdoc
  ///   cref="INoticeBatchIo.DispatchNoticesAsync(IReadOnlyDictionary{string, BatchedIoRequestNotice}, BatchDispatchOptions, CancellationToken)" />
  /// <remarks>
  ///   This implementation provides a no-operation mechanism, returning given notices as successes without processing.
  /// </remarks>
  public Task<BatchIoNoticeDispatchResult> DispatchNoticesAsync(
    IReadOnlyDictionary<string, BatchedIoRequestNotice> notices,
    BatchDispatchOptions? batchDispatchOptions = null,
    CancellationToken cancellationToken = default)
  {
    return Task.FromResult(
      new BatchIoNoticeDispatchResult
      {
        Successes = notices
          .Select(static kvp => new BatchedIoResponseNotice(kvp.Key, kvp.Value.Stream, kvp.Value.Notice))
          .ToList(),
        Failures = []
      }
    );
  }

  /// <inheritdoc cref="INoticeIo.DispatchAsync(EventStreamId, string, CancellationToken)" />
  /// <remarks>
  ///   This implementation provides a no-operation mechanism, returning the given notice without processing.
  /// </remarks>
  public Task<string> DispatchAsync(
    EventStreamId stream,
    string notice,
    CancellationToken cancellationToken = default)
  {
    return Task.FromResult(notice);
  }
}
