using Jds.NiceNotice.Dispatching;

namespace Jds.NiceNotice.Tests.Unit;

/// <summary>
///   An implementation of <see cref="INoticeBatchIo" /> which uses delegates.
/// </summary>
/// <param name="dispatchAsync"></param>
/// <param name="dispatchNoticeBatchAsync"></param>
public class DelegateBatchNoticeIo(
  Func<IoNoticeDispatchRequest, CancellationToken, Task<IoNoticeDispatchResult>> dispatchAsync,
  Func<IoBatchNoticeDispatchRequest, CancellationToken,
    Task<IoBatchNoticeDispatchResult>> dispatchNoticeBatchAsync
) : DelegateNoticeIo(dispatchAsync), INoticeBatchIo
{
  public Task<IoBatchNoticeDispatchResult> DispatchNoticesAsync(
    IoBatchNoticeDispatchRequest request,
    CancellationToken cancellationToken = default)
  {
    return dispatchNoticeBatchAsync(request, cancellationToken);
  }

  public static DelegateBatchNoticeIo AlwaysFails_Batch()
  {
    return new DelegateBatchNoticeIo(
      static (notice, cancellationToken) => throw new Exception(message: "Dispatch failed"),
      static (request, cancellationToken) => throw new Exception(message: "Dispatch failed")
    );
  }
}

/// <summary>
///   An implementation of <see cref="INoticeIo" /> which uses a delegate.
/// </summary>
/// <param name="dispatchAsync"></param>
public class DelegateNoticeIo(
  Func<IoNoticeDispatchRequest, CancellationToken, Task<IoNoticeDispatchResult>> dispatchAsync
) : INoticeIo
{
  public Task<IoNoticeDispatchResult> DispatchAsync(
    IoNoticeDispatchRequest notice,
    CancellationToken cancellationToken = default)
  {
    return dispatchAsync(notice, cancellationToken);
  }

  public static DelegateNoticeIo AlwaysFails()
  {
    return new DelegateNoticeIo(static (notice, cancellationToken) =>
      throw new Exception(message: "Dispatch failed")
    );
  }

  public async Task<string> DispatchAsync(
    EventStreamId stream,
    string notice,
    CancellationToken cancellationToken = default)
  {
    return (await dispatchAsync(IoNoticeDispatchRequest.Create(stream, notice), cancellationToken)).Notice;
  }
}
