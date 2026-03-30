namespace Jds.NiceNotice.Dispatching;

/// <summary>
///   The result of dispatching a batch of notices.
/// </summary>
public class IoBatchNoticeDispatchResult
{
  private readonly List<IoBatchNoticeDispatchResultItem> _results;

  /// <summary>
  ///   Constructs a new instance of <see cref="IoBatchNoticeDispatchResult" />.
  /// </summary>
  /// <param name="dispatchResults">The notice dispatch results, both successes and failures.</param>
  public IoBatchNoticeDispatchResult(IEnumerable<IoBatchNoticeDispatchResultItem> dispatchResults)
  {
    _results = dispatchResults.ToList();
  }

  /// <summary>
  ///   Gets the notices that failed to dispatch (<see cref="IoNoticeDispatchResult.IsSuccessful" /> is <c>false</c>).
  /// </summary>
  public IEnumerable<IoBatchNoticeDispatchResultItem> Failures => _results.Where(static r => !r.IsSuccessful);

  /// <summary>
  ///   Gets the notice dispatch results, both successes and failures.
  /// </summary>
  public IReadOnlyList<IoBatchNoticeDispatchResultItem> Results => _results;

  /// <summary>
  ///   Gets the notices that were successfully dispatched
  ///   (<see cref="IoNoticeDispatchResult.IsSuccessful" /> is <c>true</c>).
  /// </summary>
  public IEnumerable<IoBatchNoticeDispatchResultItem> Successes => _results.Where(static r => r.IsSuccessful);
}
