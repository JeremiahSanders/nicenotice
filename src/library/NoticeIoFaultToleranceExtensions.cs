namespace Jds.NiceNotice;

/// <summary>
///   Methods extending <see cref="INoticeIo" /> supporting fault tolerance.
/// </summary>
public static class NoticeIoFaultToleranceExtensions
{
  /// <summary>
  ///   Asynchronously sends a notification using this I/O dispatcher, catching exceptions.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Invokes <see cref="INoticeIo.DispatchAsync" />
  ///     and catches any exceptions thrown.
  ///     If an exception is thrown, the <paramref name="exceptionHandler" /> is invoked with the notice and the exception.
  ///   </para>
  /// </remarks>
  /// <param name="noticeIo">This instance of notice I/O.</param>
  /// <param name="stream">A logical notice event stream identifier.</param>
  /// <param name="notice">A notice to be dispatched.</param>
  /// <param name="exceptionHandler">Optional. Executed if an exception occurs when dispatching the notice.</param>
  /// <param name="cancellationToken">Optional. An asynchronous operation cancellation token.</param>
  /// <returns>Returns the result of the asynchronous operation, when successful, or null upon failure.</returns>
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
