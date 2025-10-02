namespace Jds.NiceNotice;

public static class FaultToleranceExtensions
{
  public static async Task<TypedNoticeDispatchResult<TNotice>?> TryDispatchAsync<TBaseNotice, TNotice>(
    this ITypedNoticeDispatcher<TBaseNotice> dispatcher,
    TNotice notice,
    Action<TNotice, Exception>? exceptionHandler = null,
    CancellationToken cancellationToken = default
  ) where TBaseNotice : notnull
    where TNotice : TBaseNotice
  {
    try
    {
      return await dispatcher.DispatchAsync(notice, cancellationToken);
    }
    catch (Exception ex)
    {
      exceptionHandler?.Invoke(notice, ex);

      return null;
    }
  }

  public static async Task<string?> TryDispatchAsync(
    this INoticeIo noticeIo,
    EventStreamId stream,
    string notice,
    Action<string, Exception>? exceptionHandler = null,
    CancellationToken cancellationToken = default
  )
  {
    try
    {
      return await noticeIo.DispatchAsync(stream, notice, cancellationToken);
    }
    catch (Exception ex)
    {
      exceptionHandler?.Invoke(notice, ex);

      return null;
    }
  }
}
