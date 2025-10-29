namespace Jds.NiceNotice;

/// <summary>
///   Options configuring the behavior of batch dispatch.
/// </summary>
public record BatchDispatchOptions
{
  private readonly int _maxDegreeOfParallelism = 1;

  /// <summary>
  ///   Gets a value indicating the maximum degree of parallelism to use when dispatching in parallel.
  ///   A value of <c>-1</c> is unlimited parallelism, and a value of <c>1</c> is serial dispatch.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     This value directly corresponds to <see cref="ParallelOptions.MaxDegreeOfParallelism" />.
  ///     If your <see cref="INoticeIo" /> is not thread-safe, DO NOT SET THIS VALUE TO ANYTHING OTHER THAN 1.
  ///   </para>
  ///   <para>
  ///     Note that implementations of <see cref="INoticeBatchIo" /> may choose to ignore this value.
  ///     Further, implementations might not support parallel dispatch.
  ///   </para>
  ///   <para>
  ///     If your <see cref="INoticeIo" /> implementation does NOT support batched dispatch
  ///     (does not implement <see cref="INoticeBatchIo" />)...
  ///     The default <see cref="TypedNoticeDispatcher{TEnterpriseEventBaseType}" />
  ///     and <see cref="TypedNoticeDispatcher" /> will use this to when invoking
  ///     your <see cref="INoticeIo.DispatchAsync" /> in parallel.
  ///   </para>
  /// </remarks>
  public int MaxDegreeOfParallelism
  {
    get => _maxDegreeOfParallelism;
    init =>
      // MaxDegreeOfParallelism must not be 0 or less than -1. We'll default to no parallelism in those cases.
      _maxDegreeOfParallelism =
        value == 0
          ? 1
          : value < -1
            ? 1
            : value;
  }
}
