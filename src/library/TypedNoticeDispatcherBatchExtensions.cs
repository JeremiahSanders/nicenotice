using Jds.NiceNotice.Dispatching;
using Jds.NiceNotice.TypedNotices;

namespace Jds.NiceNotice;

/// <summary>
///   Extensions to typed notice dispatchers supporting batch notice dispatch.
/// </summary>
public static class TypedNoticeDispatcherBatchExtensions
{
  #region GenericTypedDispatcher

  /// <summary>
  ///   Creates a <see cref="DispatchBatchRequest{TBaseEnterpriseEvent}" />
  ///   from <paramref name="notices" /> and sends
  ///   the result using
  ///   <see cref="ITypedNoticeDispatcher{TEnterpriseEventBaseType}.DispatchBatchAsync{TEventType}" />.
  /// </summary>
  /// <remarks>
  ///   This overload generates unique identifiers for the notices.
  ///   If <typeparamref name="TEnterpriseEventBaseType" /> extends <see cref="EnterpriseEvent" />,
  ///   then <see cref="EnterpriseEvent.Id" /> is used; otherwise, a new <see cref="Guid" /> is generated.
  /// </remarks>
  /// <param name="dispatcher">A dispatcher.</param>
  /// <param name="notices">Notices to dispatch.</param>
  /// <param name="batchDispatchOptions">
  ///   Batch dispatch options. If provided, will be passed to the configured
  ///   <see cref="INoticeBatchIo" />. Optional.
  /// </param>
  /// <param name="cancellationToken">An async operation cancellation token.</param>
  /// <typeparam name="TEnterpriseEventBaseType">A base enterprise event type.</typeparam>
  /// <returns>Returns the typed notice dispatch result.</returns>
  public static Task<BatchTypedNoticeDispatchResult> DispatchBatchAsync<TEnterpriseEventBaseType>(
    this ITypedNoticeDispatcher<TEnterpriseEventBaseType> dispatcher,
    IEnumerable<TEnterpriseEventBaseType> notices,
    BatchDispatchOptions? batchDispatchOptions = null,
    CancellationToken cancellationToken = default
  )
    where TEnterpriseEventBaseType : notnull
  {
    return dispatcher.DispatchBatchAsync(
      DispatchBatchRequest<TEnterpriseEventBaseType>.CreateFromTypedNotices(
        notices,
        batchDispatchOptions
      ),
      cancellationToken
    );
  }

  #endregion

  #region NonGenericTypedDispatcher

  /// <summary>
  ///   Creates a <see cref="DispatchBatchRequest" />
  ///   which sends all the provided <paramref name="notices" />
  ///   to the provided <paramref name="stream" />
  ///   and sends the result using
  ///   <see cref="ITypedNoticeDispatcher.DispatchBatchAsync" />.
  /// </summary>
  /// <param name="dispatcher">A typed notice dispatcher.</param>
  /// <param name="stream">A logical event stream destination.</param>
  /// <param name="notices">A sequence of notices to dispatch.</param>
  /// <param name="batchIdProvider">
  ///   A function which generates unique identifiers for each
  ///   element of the notification batch.
  ///   Optional. Default: <see cref="Guid.NewGuid" />
  /// </param>
  /// <param name="options">Batch dispatch options. Optional.</param>
  /// <param name="cancellationToken">An asynchronous operation cancellation token.</param>
  /// <returns>Returns the typed notice dispatch result.</returns>
  public static Task<BatchTypedNoticeDispatchResult> DispatchBatchToSingleStreamAsync(
    this ITypedNoticeDispatcher dispatcher,
    EventStreamId stream,
    IEnumerable<object> notices,
    Func<BatchRoutedTypedNoticeRequest, string>? batchIdProvider = null,
    BatchDispatchOptions? options = null,
    CancellationToken cancellationToken = default
  )
  {
    return dispatcher.DispatchBatchAsync(
      DispatchBatchRequest.CreateForSingleStream(
        stream,
        notices,
        batchIdProvider,
        options
      ),
      cancellationToken
    );
  }

  /// <summary>
  ///   Creates a <see cref="DispatchBatchRequest" />
  ///   which sends all the provided routed
  ///   <paramref name="notices" /> using
  ///   <see cref="ITypedNoticeDispatcher.DispatchBatchAsync" />.
  /// </summary>
  /// <param name="dispatcher">A typed notice dispatcher.</param>
  /// <param name="notices">A sequence of routed notices to dispatch.</param>
  /// <param name="batchIdProvider">
  ///   A function which generates unique identifiers for each
  ///   element of the notification batch.
  ///   Optional. Default: <see cref="Guid.NewGuid" />
  /// </param>
  /// <param name="options">Batch dispatch options. Optional.</param>
  /// <param name="cancellationToken">An asynchronous operation cancellation token.</param>
  /// <returns>Returns the typed notice dispatch result.</returns>
  public static Task<BatchTypedNoticeDispatchResult> DispatchBatchFromRoutedNoticesAsync(
    this ITypedNoticeDispatcher dispatcher,
    IEnumerable<BatchRoutedTypedNoticeRequest> notices,
    Func<BatchRoutedTypedNoticeRequest, string>? batchIdProvider = null,
    BatchDispatchOptions? options = null,
    CancellationToken cancellationToken = default
  )
  {
    return dispatcher.DispatchBatchAsync(
      DispatchBatchRequest.CreateFromRoutedNotices(
        notices,
        batchIdProvider,
        options
      ),
      cancellationToken
    );
  }

  #endregion
}
