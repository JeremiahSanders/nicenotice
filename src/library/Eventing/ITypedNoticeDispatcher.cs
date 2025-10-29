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
///   <para>
///     When creating a custom implementation of this interface,
///     use <see cref="Jds.NiceNotice.TypedNoticeDispatcher{TEnterpriseEventBaseType}" /> for easy implementation.
///   </para>
/// </remarks>
/// <typeparam name="TEnterpriseEventBaseType">
///   The base type of enterprise event that can be dispatched by the implementation.
///   The type must be a non-nullable type.
/// </typeparam>
public interface ITypedNoticeDispatcher<in TEnterpriseEventBaseType>
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

  /// <summary>
  ///   Dispatches a batch of enterprise event notices to logical event streams
  ///   using the configured <see cref="INoticeIo" />.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     The default implementation applies (in order):
  ///     (1) logical routing, (2) serialization, (3) validation,
  ///     and (4) dispatch to I/O using an <see cref="INoticeIo" />.
  ///   </para>
  /// </remarks>
  /// <param name="request">A batch notice dispatch request.</param>
  /// <param name="cancellationToken">An asynchronous operation cancellation token.</param>
  /// <typeparam name="TEventType">
  ///   A notice event type. Must be a non-nullable type which inherits from
  ///   <typeparamref name="TEnterpriseEventBaseType" />.
  ///   By default, this generic type argument implies that all notices are of the same type.
  ///   However, if you invoke this method with a common base type, such as <typeparamref name="TEnterpriseEventBaseType" />,
  ///   then notices of different types can be dispatched together in a single batch.
  /// </typeparam>
  /// <returns>Returns the result of dispatching the batch of notices.</returns>
  Task<BatchTypedNoticeDispatchResult> DispatchBatchAsync<TEventType>(
    DispatchBatchRequest<TEventType> request,
    CancellationToken cancellationToken = default)
    where TEventType : TEnterpriseEventBaseType;
}

/// <summary>
///   Defines an interface for dispatching enterprise events to logical event streams.
///   The default implementation applies (in order):
///   (1) serialization, (2) validation, and (3) dispatch to I/O using an <see cref="INoticeIo" />.
/// </summary>
/// <remarks>
///   <para>
///     When creating a custom implementation of this interface,
///     use <see cref="Jds.NiceNotice.TypedNoticeDispatcher" /> for easy implementation.
///   </para>
/// </remarks>
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
  /// <param name="notice">A notice to dispatch.</param>
  /// <param name="streamId">A logical stream to which the <paramref name="notice" /> is being dispatched.</param>
  /// <param name="cancellationToken">An asynchronous operation cancellation token.</param>
  /// <typeparam name="TEventType">A notice type.</typeparam>
  /// <returns>Returns the result of dispatching the notice.</returns>
  Task<TypedNoticeDispatchResult<TEventType>> DispatchAsync<TEventType>(
    TEventType notice,
    EventStreamId streamId,
    CancellationToken cancellationToken = default
  )
    where TEventType : notnull;

  /// <summary>
  ///   Dispatches a batch of enterprise event notices to logical event streams.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     The default implementation applies (in order):
  ///     (1) serialization, (2) validation, and (3) dispatch to I/O using an <see cref="INoticeIo" />.
  ///   </para>
  /// </remarks>
  /// <param name="request">A request containing the notices to dispatch.</param>
  /// <param name="cancellationToken">An asynchronous operation cancellation token.</param>
  /// <returns>Returns the result of dispatching the batch of notices.</returns>
  Task<BatchTypedNoticeDispatchResult> DispatchBatchAsync(
    DispatchBatchRequest request,
    CancellationToken cancellationToken = default
  );
}
