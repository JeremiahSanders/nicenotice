using Jds.NiceNotice.Dispatching;

namespace Jds.NiceNotice;

/// <summary>
///   A <see cref="INoticeIo" /> that can dispatch batches of notices.
/// </summary>
public interface INoticeBatchIo : INoticeIo
{
  /// <summary>
  ///   Dispatch a batch of event notices.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Due to the inherent complexities to batched and/or parallel work and aggregating results,
  ///     implementations of this interface may vary in their logic.
  ///   </para>
  /// </remarks>
  /// <param name="batchIoRequest">A dispatch request.</param>
  /// <param name="cancellationToken">Optional. An asynchronous operation cancellation token.</param>
  /// <returns>
  ///   A task representing the asynchronous operation,
  ///   containing the result of the batch dispatch operation.
  /// </returns>
  Task<BatchIoNoticeDispatchResult> DispatchNoticesAsync(
    BatchIoRequest batchIoRequest,
    CancellationToken cancellationToken = default);
}
