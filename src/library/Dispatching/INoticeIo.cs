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
  ///   Dispatches an asynchronous event to the specified event stream with the given notice.
  /// </summary>
  /// <remarks>This is the core</remarks>
  /// <param name="stream">
  ///   <para>The event stream ID where the notice will be dispatched.</para>
  ///   <para>
  ///     This is a logical stream; it does not inherently relate to any specific I/O destination
  ///     (e.g., filename, URL, database table).
  ///   </para>
  ///   <para>Implementations of <see cref="INoticeIo" /> are expected to implement any needed routing logic.</para>
  /// </param>
  /// <param name="notice">The content of the notice to be dispatched.</param>
  /// <param name="cancellationToken">An asynchronous operation cancellation token.</param>
  /// <returns>
  ///   A task representing the asynchronous operation,
  ///   containing <paramref name="notice" /> after successful completion.
  /// </returns>
  Task<string> DispatchAsync(EventStreamId stream, string notice, CancellationToken cancellationToken = default);
}
