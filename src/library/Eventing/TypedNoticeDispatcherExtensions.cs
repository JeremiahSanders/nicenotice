namespace Jds.NiceNotice;

public static class TypedNoticeDispatcherExtensions
{
  public static Task<TypedNoticeDispatchResult<TEventType>> DispatchAsync<TEventType>(
    this ITypedNoticeDispatcher dispatcher,
    TEventType notice,
    Func<TEventType, EventStreamId> streamSelector,
    CancellationToken cancellationToken = default) where TEventType : notnull
  {
    return dispatcher.DispatchAsync(notice, streamSelector(notice), cancellationToken);
  }

  public static Task<TypedNoticeDispatchResult<TEventType>> DispatchAsync<TEventType>(
    this ITypedNoticeDispatcher dispatcher,
    TEventType notice,
    bool dispatchToFullNameStream = false,
    CancellationToken cancellationToken = default) where TEventType : notnull
  {
    return dispatcher.DispatchAsync(
      notice,
      dispatchToFullNameStream
        ? StreamSelectors.FullNameStreamProvider(notice)
        : StreamSelectors.TypeNameStreamProvider(notice),
      cancellationToken
    );
  }
}