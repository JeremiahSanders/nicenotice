namespace Jds.NiceNotice.TypedNotices;

/// <summary>
///   The result of dispatching a batch of typed notices.
/// </summary>
public class BatchTypedNoticeDispatchResult
{
  /// <summary>
  ///   Gets the notices that were successfully dispatched.
  /// </summary>
  public required IReadOnlyList<BatchRoutedTypedNoticeResponse> Successes { get; init; }

  /// <summary>
  ///   Gets the notices that failed to dispatch.
  /// </summary>
  public required IReadOnlyList<(BatchRoutedTypedNoticeResponse, Exception)> Failures { get; init; }
}
