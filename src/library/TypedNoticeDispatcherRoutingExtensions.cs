using Jds.NiceNotice.TypedNotices;
using Jds.NiceNotice.TypedNotices.Routing;

namespace Jds.NiceNotice;

/// <summary>
///   Methods extending <see cref="ITypedNoticeDispatcher" /> to support additional typed notice dispatch patterns.
/// </summary>
public static class TypedNoticeDispatcherRoutingExtensions
{
  /// <summary>
  ///   Asynchronously dispatches a notice to a specified event stream.
  /// </summary>
  /// <param name="dispatcher">This notice dispatcher, which will handle the dispatch operation.</param>
  /// <param name="notice">The notice to be dispatched.</param>
  /// <param name="streamSelector">
  ///   A function that maps the notice to an event stream identifier.
  ///   I.e., it determines the event stream to which the notice should be dispatched.
  /// </param>
  /// <param name="cancellationToken">Optional. An asynchronous operation cancellation token.</param>
  /// <typeparam name="TEventType">The type of the notice being dispatched.</typeparam>
  /// <returns>A task that represents the asynchronous operation. The task result contains the dispatch result.</returns>
  public static Task<TypedNoticeDispatchResult<TEventType>> DispatchAsync<TEventType>(
    this ITypedNoticeDispatcher dispatcher,
    TEventType notice,
    Func<TEventType, EventStreamId> streamSelector,
    CancellationToken cancellationToken = default) where TEventType : notnull
  {
    return dispatcher.DispatchAsync(notice, streamSelector(notice), cancellationToken);
  }

  /// <summary>
  ///   Asynchronously dispatches a notice to an event stream based on the type of the notice.
  /// </summary>
  /// <param name="dispatcher">This notice dispatcher, which will handle the dispatch operation.</param>
  /// <param name="notice">The notice to be dispatched.</param>
  /// <param name="dispatchToFullNameStream">
  ///   A value indicating whether the stream name is based on the type name (e.g., <c>MyEvent</c>)
  ///   or the full type name (e.g., <c>MyCompany.MyApp.MyEvent</c>).
  /// </param>
  /// <param name="cancellationToken">Optional. An asynchronous operation cancellation token.</param>
  /// <typeparam name="TEventType">A notice object type.</typeparam>
  /// <returns>A task that represents the asynchronous operation. The task result contains the dispatch result.</returns>
  public static Task<TypedNoticeDispatchResult<TEventType>> DispatchAsync<TEventType>(
    this ITypedNoticeDispatcher dispatcher,
    TEventType notice,
    bool dispatchToFullNameStream = false,
    CancellationToken cancellationToken = default) where TEventType : notnull
  {
    return dispatcher.DispatchAsync(
      notice,
      dispatchToFullNameStream
        ? Routers.FullNameStreamProvider(notice)
        : Routers.TypeNameStreamProvider(notice),
      cancellationToken
    );
  }
}
