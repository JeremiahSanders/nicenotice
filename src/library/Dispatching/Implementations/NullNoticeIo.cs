namespace Jds.NiceNotice.Dispatching.Implementations;

/// <summary>
///   Provides a no-operation implementation of <see cref="INoticeIo" />,
///   primarily used as a default or placeholder where event dispatching is not required.
/// </summary>
public class NullNoticeIo : INoticeIo, INoticeBatchIo
{
  /// <inheritdoc
  ///   cref="INoticeBatchIo.DispatchNoticesAsync" />
  /// <remarks>
  ///   This implementation provides a no-operation mechanism, returning given notices as successes without processing.
  /// </remarks>
  public Task<BatchIoNoticeDispatchResult> DispatchNoticesAsync(
    BatchIoRequest request,
    CancellationToken cancellationToken = default)
  {
    return Task.FromResult(
      new BatchIoNoticeDispatchResult(
        request
          .Notices
          .Select(static kvp => new BatchedIoResponseNotice(
              kvp.Key,
              kvp.Value.Stream,
              kvp.Value.Notice,
              kvp.Value.Metadata,
              kvp.Value.ContentType,
              exception: null
            )
          )
      )
    );
  }

  /// <inheritdoc cref="INoticeIo.DispatchAsync(IoRequestNotice, CancellationToken)" />
  /// <remarks>
  ///   This implementation provides a no-operation mechanism, returning the given notice without processing.
  /// </remarks>
  public Task<IoNoticeDispatchResult> DispatchAsync(
    IoRequestNotice notice,
    CancellationToken cancellationToken = default)
  {
    return Task.FromResult(
      new IoNoticeDispatchResult(notice.Stream, notice.Notice, notice.Metadata, notice.ContentType, exception: null)
    );
  }
}
