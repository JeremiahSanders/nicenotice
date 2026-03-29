namespace Jds.NiceNotice.TypedNotices;

/// <summary>
///   The result of dispatching a batch of typed notices.
/// </summary>
public class BatchTypedNoticeDispatchResult(IEnumerable<BatchRoutedTypedNoticeResponse> results)
{
  /// <summary>
  ///   Gets the notices that failed to dispatch.
  /// </summary>
  public IEnumerable<BatchRoutedTypedNoticeResponse> Failures =>
    Results.Where(static response => !response.IsSuccessful);

  /// <summary>
  ///   Gets the notices that were dispatched.
  /// </summary>
  public IReadOnlyList<BatchRoutedTypedNoticeResponse> Results { get; } = results.ToList();

  /// <summary>
  ///   Gets the notices that were successfully dispatched.
  /// </summary>
  public IEnumerable<BatchRoutedTypedNoticeResponse> Successes =>
    Results.Where(static response => response.IsSuccessful);

  /// <summary>
  ///   Ensures that all notices were dispatched successfully, throwing an <see cref="IOException" /> if not.
  /// </summary>
  /// <returns>Returns this instance.</returns>
  /// <exception cref="IOException">Thrown if any notice failed to dispatch.</exception>
  public BatchTypedNoticeDispatchResult RequireSuccess()
  {
    List<BatchRoutedTypedNoticeResponse> failures = Results
      .Where(static response => response.Exception is not null)
      .ToList();

    if (failures.Count <= 0)
    {
      return this;
    }

    if (failures.Count == 1)
    {
      BatchRoutedTypedNoticeResponse one = failures[index: 0];

      throw new IOException(
        $"Failed to dispatch notice with {nameof(one.BatchNoticeId)} '{one.BatchNoticeId}'.",
        one.Exception
      );
    }

    List<Exception> exceptions = failures.Select(static response => response.Exception).OfType<Exception>().ToList();
    string batchNoticeIds = string.Join(separator: " ; ", failures.Select(static response => response.BatchNoticeId));

    throw new IOException(
      $"Failed to dispatch {failures.Count} notices. ({nameof(BatchRoutedTypedNoticeResponse.BatchNoticeId)}s: {batchNoticeIds})",
      new AggregateException(exceptions)
    );
  }
}
