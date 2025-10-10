namespace Jds.NiceNotice;

/// <summary>
///   Defines an interface for dispatching enterprise events of specified base types
///   to logical event streams.
///   The default implementation applies (in order):
///   (1) logical routing, (2) serialization, (3) validation,
///   and (4) dispatch to I/O using an <see cref="INoticeIo" />.
/// </summary>
/// <remarks>
///   <para>
///     This is the primary type dependency used in runtime applications when using NiceNotice.
///     I.e., the type that is injected into the application's business logic via constructor.
///   </para>
/// </remarks>
/// <typeparam name="TEnterpriseEventBaseType">
///   The base type of enterprise event that can be dispatched by the implementation.
///   The type must be a non-nullable type.
/// </typeparam>
public interface ITypedNoticeDispatcher<TEnterpriseEventBaseType>
  where TEnterpriseEventBaseType : notnull
{
  /// <summary>
  ///   Dispatch an enterprise event notice to the appropriate event stream using the configured <see cref="INoticeIo" />.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     The default implementation applies (in order):
  ///     (1) logical routing, (2) serialization, (3) validation,
  ///     and (4) dispatch to I/O using an <see cref="INoticeIo" />.
  ///   </para>
  /// </remarks>
  /// <param name="notice">The notice being dispatched.</param>
  /// <param name="cancellationToken">Optional. An asynchronous operation cancellation token.</param>
  /// <typeparam name="TEventType">
  ///   The notice event type. Must be a non-nullable type which inherits from
  ///   <typeparamref name="TEnterpriseEventBaseType" />.
  /// </typeparam>
  /// <returns>Returns a task representing the asynchronous operation.</returns>
  Task<TypedNoticeDispatchResult<TEventType>> DispatchAsync<TEventType>(
    TEventType notice,
    CancellationToken cancellationToken = default)
    where TEventType : TEnterpriseEventBaseType;
}

/// <summary>
///   Defines an interface for dispatching enterprise events to logical event streams.
///   The default implementation applies (in order):
///   (1) serialization, (2) validation, and (3) dispatch to I/O using an <see cref="INoticeIo" />.
/// </summary>
public interface ITypedNoticeDispatcher
{
  /// <summary>
  ///   Dispatches an enterprise event notice to a specific event stream.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     The default implementation applies (in order):
  ///     (1) serialization, (2) validation, and (3) dispatch to I/O using an <see cref="INoticeIo" />.
  ///   </para>
  /// </remarks>
  /// <param name="notice"></param>
  /// <param name="streamId"></param>
  /// <param name="cancellationToken"></param>
  /// <typeparam name="TEventType"></typeparam>
  /// <returns></returns>
  Task<TypedNoticeDispatchResult<TEventType>> DispatchAsync<TEventType>(
    TEventType notice,
    EventStreamId streamId,
    CancellationToken cancellationToken = default)
    where TEventType : notnull;
}
