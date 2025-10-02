namespace Jds.NiceNotice;

/// <summary>
///   Defines an interface for dispatching enterprise events of specified base types
///   to corresponding event streams.
/// </summary>
/// <typeparam name="TEnterpriseEventBaseType">
///   The base type of enterprise event that can be dispatched by the implementation.
/// </typeparam>
public interface ITypedNoticeDispatcher<TEnterpriseEventBaseType>
  where TEnterpriseEventBaseType : notnull
{
  public Task<TypedNoticeDispatchResult<TEventType>> DispatchAsync<TEventType>(
    TEventType notice,
    CancellationToken cancellationToken = default)
    where TEventType : TEnterpriseEventBaseType;
}

public interface ITypedNoticeDispatcher
{
  public Task<TypedNoticeDispatchResult<TEventType>> DispatchAsync<TEventType>(
    TEventType notice,
    EventStreamId streamId,
    CancellationToken cancellationToken = default)
    where TEventType : notnull;
}
