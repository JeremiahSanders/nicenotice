namespace Jds.NiceNotice;

/// <summary>
///   Represents a dispatcher responsible for sending enterprise event notices to specific event streams.
/// </summary>
/// <remarks>
///   <para>
///     This type is a lowest-common-denominator abstraction for notification I/O
///     (e.g., RabbitMQ, AWS SNS).
///   </para>
/// </remarks>
public interface INoticeIo
{
  /// <summary>
  ///   Asynchronously dispatches a notice to an I/O destination.
  /// </summary>
  /// <param name="notice">An I/O request wrapper for a notice to be dispatched.</param>
  /// <param name="cancellationToken">An asynchronous operation cancellation token.</param>
  /// <returns>
  ///   A task representing the asynchronous operation,
  ///   containing <paramref name="notice" /> after successful completion.
  /// </returns>
  Task<IoNoticeDispatchResult> DispatchAsync(IoNoticeDispatchRequest notice, CancellationToken cancellationToken = default);
}
