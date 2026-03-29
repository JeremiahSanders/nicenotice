namespace Jds.NiceNotice.Dispatching;

/// <summary>
///   The result of dispatching a batch of notices.
/// </summary>
public class BatchIoNoticeDispatchResult
{
  private readonly List<BatchedIoResponseNotice> _results;

  /// <summary>
  ///   Constructs a new instance of <see cref="BatchIoNoticeDispatchResult" />.
  /// </summary>
  /// <param name="dispatchResults">The notice dispatch results, both successes and failures.</param>
  public BatchIoNoticeDispatchResult(IEnumerable<BatchedIoResponseNotice> dispatchResults)
  {
    _results = dispatchResults.ToList();
  }

  /// <summary>
  ///   Gets the notices that failed to dispatch (<see cref="IoNoticeDispatchResult.IsSuccessful" /> is <c>false</c>).
  /// </summary>
  public IEnumerable<BatchedIoResponseNotice> Failures => _results.Where(static r => !r.IsSuccessful);

  /// <summary>
  ///   Gets the notice dispatch results, both successes and failures.
  /// </summary>
  public IReadOnlyList<BatchedIoResponseNotice> Results => _results;

  /// <summary>
  ///   Gets the notices that were successfully dispatched
  ///   (<see cref="IoNoticeDispatchResult.IsSuccessful" /> is <c>true</c>).
  /// </summary>
  public IEnumerable<BatchedIoResponseNotice> Successes => _results.Where(static r => r.IsSuccessful);
}
