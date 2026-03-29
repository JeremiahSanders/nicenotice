using Jds.NiceNotice.TypedNotices;

namespace Jds.NiceNotice;

/// <summary>
///   Methods extending <see cref="ITypedNoticeDispatcher" /> and
///   <see cref="ITypedNoticeDispatcher{TEnterpriseEventBaseType}" />
///   supporting fault tolerance.
/// </summary>
public static class TypedNoticeDispatcherFaultToleranceExtensions
{
  #region GenericTypedDispatcher

  /// <summary>
  ///   Asynchronously sends a notification to the configured I/O dispatcher, catching exceptions.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Invokes <see cref="ITypedNoticeDispatcher{TBaseNotice}.DispatchAsync{TNotice}" />
  ///     and catches any exceptions thrown.
  ///     If an exception is thrown, the <paramref name="exceptionHandler" /> is invoked with the notice and the exception.
  ///   </para>
  /// </remarks>
  /// <param name="dispatcher">This typed notice dispatcher.</param>
  /// <param name="notice">A notice to be dispatched.</param>
  /// <param name="exceptionHandler">Optional. Executed if an exception occurs when dispatching the notice.</param>
  /// <param name="cancellationToken">Optional. An asynchronous operation cancellation token.</param>
  /// <typeparam name="TBaseNotice">A base notice type of <typeparamref name="TNotice" />.</typeparam>
  /// <typeparam name="TNotice">A notice type which is being dispatched.</typeparam>
  /// <returns>Returns the result of the asynchronous operation, when successful, or null upon failure.</returns>
  public static async Task<TypedNoticeDispatchResult<TNotice>> TryDispatchAsync<TBaseNotice, TNotice>(
    this ITypedNoticeDispatcher<TBaseNotice> dispatcher,
    TNotice notice,
    Action<TNotice, Exception>? exceptionHandler = null,
    CancellationToken cancellationToken = default
  ) where TBaseNotice : notnull
    where TNotice : TBaseNotice
  {
    return (await Eithers.TryAsync(() => dispatcher.DispatchAsync(notice, cancellationToken)))
      .TapRight(result =>
        {
          if (result.Exception != null && exceptionHandler != null)
          {
            exceptionHandler(notice, result.Exception);
          }
        }
      )
      .TapLeft(exception => exceptionHandler?.Invoke(notice, exception))
      .FoldRight(exception => new TypedNoticeDispatchResult<TNotice>
        {
          Exception = exception,
          Notice = notice,
          IoRequest = new IoRequestNotice(
            EventStreamId.From(notice.GetType().Name),
            string.Empty,
            metadata: null,
            contentType: null
          )
        }
      );
  }

  #endregion

  #region NonGenericTypedDispatcher

  /// <summary>
  ///   Asynchronously sends a notification to the configured I/O dispatcher, catching exceptions.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Invokes
  ///     <see
  ///       cref="TypedNoticeDispatcherRoutingExtensions.DispatchAsync{TEventType}(ITypedNoticeDispatcher,TEventType,bool,System.Threading.CancellationToken)" />
  ///     and catches any exceptions thrown.
  ///     If an exception is thrown, the <paramref name="exceptionHandler" /> is invoked with the notice and the exception.
  ///   </para>
  /// </remarks>
  /// <param name="dispatcher">This notice dispatcher, which will handle the dispatch operation.</param>
  /// <param name="notice">The notice to be dispatched.</param>
  /// <param name="eventStreamId">The logical stream to which the notification will be sent.</param>
  /// <param name="exceptionHandler">Optional. Executed if an exception occurs when dispatching the notice.</param>
  /// <param name="cancellationToken">Optional. An asynchronous operation cancellation token.</param>
  /// <typeparam name="TNotice">A notice object type.</typeparam>
  /// <returns>A task that represents the asynchronous operation. The task result contains the dispatch result.</returns>
  public static async Task<TypedNoticeDispatchResult<TNotice>?> TryDispatchAsync<TNotice>(
    this ITypedNoticeDispatcher dispatcher,
    TNotice notice,
    EventStreamId eventStreamId,
    Action<TNotice, Exception>? exceptionHandler = null,
    CancellationToken cancellationToken = default
  ) where TNotice : notnull
  {
    return (await Eithers.TryAsync(() => dispatcher.DispatchAsync(notice, eventStreamId, cancellationToken)))
      .TapRight(result =>
        {
          if (result.Exception != null && exceptionHandler != null)
          {
            exceptionHandler(notice, result.Exception);
          }
        }
      )
      .TapLeft(exception => exceptionHandler?.Invoke(notice, exception))
      .FoldRight(exception => new TypedNoticeDispatchResult<TNotice>
        {
          Exception = exception,
          Notice = notice,
          IoRequest = new IoRequestNotice(
            eventStreamId,
            string.Empty,
            metadata: null,
            contentType: null
          )
        }
      );
  }

  /// <summary>
  ///   Asynchronously sends a notification to the configured I/O dispatcher, catching exceptions.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Invokes
  ///     <see
  ///       cref="TypedNoticeDispatcherRoutingExtensions.DispatchAsync{TEventType}(ITypedNoticeDispatcher,TEventType,bool,System.Threading.CancellationToken)" />
  ///     and catches any exceptions thrown.
  ///     If an exception is thrown, the <paramref name="exceptionHandler" /> is invoked with the notice and the exception.
  ///   </para>
  /// </remarks>
  /// <param name="dispatcher">This notice dispatcher, which will handle the dispatch operation.</param>
  /// <param name="notice">The notice to be dispatched.</param>
  /// <param name="exceptionHandler">Optional. Executed if an exception occurs when dispatching the notice.</param>
  /// <param name="dispatchToFullNameStream">
  ///   A value indicating whether the stream name is based on the type name (e.g., <c>MyEvent</c>)
  ///   or the full type name (e.g., <c>MyCompany.MyApp.MyEvent</c>).
  /// </param>
  /// <param name="cancellationToken">Optional. An asynchronous operation cancellation token.</param>
  /// <typeparam name="TNotice">A notice object type.</typeparam>
  /// <returns>A task that represents the asynchronous operation. The task result contains the dispatch result.</returns>
  public static async Task<TypedNoticeDispatchResult<TNotice>?> TryDispatchAsync<TNotice>(
    this ITypedNoticeDispatcher dispatcher,
    TNotice notice,
    Action<TNotice, Exception>? exceptionHandler = null,
    bool dispatchToFullNameStream = false,
    CancellationToken cancellationToken = default
  ) where TNotice : notnull
  {
    return (await Eithers.TryAsync(() => dispatcher.DispatchAsync(notice, dispatchToFullNameStream, cancellationToken)))
      .TapRight(result =>
        {
          if (result.Exception != null && exceptionHandler != null)
          {
            exceptionHandler(notice, result.Exception);
          }
        }
      )
      .TapLeft(exception => exceptionHandler?.Invoke(notice, exception))
      .FoldRight(exception => new TypedNoticeDispatchResult<TNotice>
        {
          Exception = exception,
          Notice = notice,
          IoRequest = new IoRequestNotice(
            EventStreamId.From(
              dispatchToFullNameStream ? notice.GetType().FullName ?? notice.GetType().Name : notice.GetType().Name
            ),
            string.Empty,
            metadata: null,
            contentType: null
          )
        }
      );
  }

  #endregion
}
