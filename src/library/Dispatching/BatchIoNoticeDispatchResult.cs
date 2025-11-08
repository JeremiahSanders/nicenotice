namespace Jds.NiceNotice.Dispatching;

/// <summary>
///   The result of dispatching a batch of notices.
/// </summary>
public class BatchIoNoticeDispatchResult
{
  /// <summary>
  ///   Gets the notices that were successfully dispatched.
  /// </summary>
  public required IReadOnlyList<BatchedIoResponseNotice> Successes { get; init; }

  /// <summary>
  ///   Gets the notices that failed to dispatch.
  /// </summary>
  public required IReadOnlyList<(BatchedIoResponseNotice, Exception)> Failures { get; init; }
}
