namespace Jds.NiceNotice.Tests.Unit;

/// <summary>
///   An implementation of <see cref="INoticeBatchIo" /> which uses delegates.
/// </summary>
/// <param name="dispatchAsync"></param>
/// <param name="dispatchNoticeBatchAsync"></param>
public class DelegateBatchNoticeIo(
  Func<EventStreamId, string, CancellationToken, Task<string>> dispatchAsync,
  Func<IReadOnlyDictionary<string, BatchedIoRequestNotice>, BatchDispatchOptions?, CancellationToken,
    Task<BatchIoNoticeDispatchResult>> dispatchNoticeBatchAsync
) : DelegateNoticeIo(dispatchAsync), INoticeBatchIo
{
  public Task<BatchIoNoticeDispatchResult> DispatchNoticesAsync(
    IReadOnlyDictionary<string, BatchedIoRequestNotice> notices,
    BatchDispatchOptions? batchDispatchOptions = null,
    CancellationToken cancellationToken = default)
  {
    return dispatchNoticeBatchAsync(notices, batchDispatchOptions, cancellationToken);
  }

  public static DelegateBatchNoticeIo AlwaysFails_Batch()
  {
    return new DelegateBatchNoticeIo(
      static (stream, notice, cancellationToken) => throw new Exception(message: "Dispatch failed"),
      static (notices, options, cancellationToken) => throw new Exception(message: "Dispatch failed")
    );
  }
}

/// <summary>
///   An implementation of <see cref="INoticeIo" /> which uses a delegate.
/// </summary>
/// <param name="dispatchAsync"></param>
public class DelegateNoticeIo(
  Func<EventStreamId, string, CancellationToken, Task<string>> dispatchAsync
) : INoticeIo
{
  public Task<string> DispatchAsync(EventStreamId stream, string notice, CancellationToken cancellationToken = default)
  {
    return dispatchAsync(stream, notice, cancellationToken);
  }

  public static DelegateNoticeIo AlwaysFails()
  {
    return new DelegateNoticeIo(static (stream, notice, cancellationToken) =>
      throw new Exception(message: "Dispatch failed")
    );
  }
}
