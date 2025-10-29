namespace Jds.NiceNotice;

/// <summary>
///   A request to dispatch a batch of notices.
/// </summary>
/// <remarks>This request is intended for use with <see cref="ITypedNoticeDispatcher.DispatchBatchAsync" />.</remarks>
public class DispatchBatchRequest : DispatchBatchRequest<BatchedRoutedTypedNotice>
{
  /// <summary>
  ///   Creates a new instance of <see cref="DispatchBatchRequest" />.
  ///   Unique identifiers (to identifier elements within the batch) are generated for each notice.
  /// </summary>
  /// <param name="notices">A sequence of notices to dispatch.</param>
  /// <param name="options">Optional. Batch dispatch configuration options.</param>
  /// <returns>Returns the created request.</returns>
  public static new DispatchBatchRequest Create(
    IEnumerable<BatchedRoutedTypedNotice> notices,
    BatchDispatchOptions? options = null
  )
  {
    return new DispatchBatchRequest
    {
      Notices = notices.ToDictionary(_ => Guid.NewGuid().ToString(), notice => notice),
      BatchDispatchOptions = options
    };
  }

  /// <summary>
  ///   Creates a new instance of <see cref="DispatchBatchRequest" />.
  ///   All notices are routed to the same stream, <paramref name="stream" />.
  ///   Unique identifiers (to identifier elements within the batch) are generated for each notice.
  /// </summary>
  /// <param name="stream"></param>
  /// <param name="notices"></param>
  /// <param name="options"></param>
  /// <returns></returns>
  public static DispatchBatchRequest Create(
    EventStreamId stream,
    IEnumerable<object> notices,
    BatchDispatchOptions? options = null)
  {
    return Create(notices.Select(notice => new BatchedRoutedTypedNotice(stream, notice)), options);
  }
}

/// <summary>
///   A request to dispatch a batch of notices.
/// </summary>
/// <remarks>
///   This request is intended for use with
///   <see cref="ITypedNoticeDispatcher{TEnterpriseEventBaseType}.DispatchBatchAsync" />.
/// </remarks>
public class DispatchBatchRequest<TBaseEnterpriseEvent>
{
  /// <summary>
  ///   Gets the notices which are being dispatched.
  ///   Key is a unique identifier for the notice within the batch, which is used to correlate responses.
  ///   Value is the notice itself.
  /// </summary>
  public required IReadOnlyDictionary<string, TBaseEnterpriseEvent> Notices { get; init; }

  /// <summary>
  ///   Gets the batch dispatch options.
  /// </summary>
  public BatchDispatchOptions? BatchDispatchOptions { get; init; }

  /// <summary>
  ///   Creates a new instance of <see cref="DispatchBatchRequest" />.
  ///   Unique identifiers (to identifier elements within the batch) are generated for each notice.
  /// </summary>
  /// <param name="notices">A sequence of notices to dispatch.</param>
  /// <param name="options">Optional. Batch dispatch configuration options.</param>
  /// <returns>Returns the created request.</returns>
  public static DispatchBatchRequest<TBaseEnterpriseEvent> Create(
    IEnumerable<TBaseEnterpriseEvent> notices,
    BatchDispatchOptions? options = null
  )
  {
    return new DispatchBatchRequest<TBaseEnterpriseEvent>
    {
      Notices = notices.ToDictionary(_ => Guid.NewGuid().ToString(), notice => notice),
      BatchDispatchOptions = options
    };
  }
}
