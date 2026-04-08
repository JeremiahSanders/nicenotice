using Jds.NiceNotice.Dispatching;
using Jds.NiceNotice.TypedNotices.Routing;

namespace Jds.NiceNotice.TypedNotices;

/// <summary>
///   A request to dispatch a batch of notices.
/// </summary>
/// <remarks>This request is intended for use with <see cref="ITypedNoticeDispatcher.DispatchBatchAsync" />.</remarks>
public class BatchDispatchRequest : BatchDispatchRequest<BatchRoutedTypedNoticeRequest>
{
  /// <summary>
  ///   Creates a new instance of <see cref="BatchDispatchRequest" />.
  ///   Notices are routed to streams inferred from their type metadata.
  ///   Preference is given to the <see cref="NoticeStreamAttribute" /> on the notice type (or in its type hierarchy).
  ///   If no attribute is present, the notices are routed to streams from their type name.
  ///   Unique identifiers (to identifier elements within the batch) are generated for each notice.
  /// </summary>
  /// <param name="notices">A sequence of notices which are being dispatched.</param>
  /// <param name="defaultToFullTypeName">
  ///   A value indicating whether to default to the full type name (e.g., <c>MyCompany.MyApp.MyEvent</c>).
  ///   When <c>false</c>, the type name (e.g., <c>MyEvent</c>) is used.
  /// </param>
  /// <param name="batchIdProvider">
  ///   A function which generates unique identifiers for each notice within the batch.
  ///   Optional. Default: <see cref="Guid.NewGuid" />.
  /// </param>
  /// <param name="options">Optional. Batch dispatch configuration options.</param>
  /// <returns>Returns the created request.</returns>
  /// <exception cref="ArgumentException">
  ///   Thrown if identities provided by <paramref name="batchIdProvider" />
  ///   cannot be used to create a request dictionary.
  /// </exception>
  public static BatchDispatchRequest CreateForInferredRoutes(
    IEnumerable<object> notices,
    bool defaultToFullTypeName = false,
    Func<BatchRoutedTypedNoticeRequest, string>? batchIdProvider = null,
    BatchDispatchOptions? options = null
  )
  {
    return CreateFromRoutedNotices(
      notices
        .Select(notice =>
          {
            EventStreamId route =
              defaultToFullTypeName
                ? Routers.DelegateAlgorithms.AttributeOrFullNameStreamProvider(notice)
                : Routers.DelegateAlgorithms.AttributeOrTypeNameStreamProvider(notice);

            return new BatchRoutedTypedNoticeRequest(route, notice);
          }
        ),
      batchIdProvider,
      options
    );
  }

  /// <summary>
  ///   Creates a new instance of <see cref="BatchDispatchRequest" />.
  ///   All notices are routed to the same stream, <paramref name="stream" />.
  ///   Unique identifiers (to identifier elements within the batch) are generated for each notice.
  /// </summary>
  /// <param name="stream">A logical stream where the <paramref name="notices" /> are dispatched.</param>
  /// <param name="notices">A sequence of notices which are being dispatched.</param>
  /// <param name="batchIdProvider">
  ///   A function which generates unique identifiers for each notice within the batch.
  ///   Optional. Default: <see cref="Guid.NewGuid" />.
  /// </param>
  /// <param name="options">Optional. Batch dispatch configuration options.</param>
  /// <returns>Returns the created request.</returns>
  /// <exception cref="ArgumentException">
  ///   Thrown if identities provided by <paramref name="batchIdProvider" />
  ///   cannot be used to create a request dictionary.
  /// </exception>
  public static BatchDispatchRequest CreateForSingleStream(
    EventStreamId stream,
    IEnumerable<object> notices,
    Func<BatchRoutedTypedNoticeRequest, string>? batchIdProvider = null,
    BatchDispatchOptions? options = null)
  {
    return CreateFromRoutedNotices(
      notices.Select(notice => new BatchRoutedTypedNoticeRequest(stream, notice)),
      batchIdProvider,
      options
    );
  }

  /// <summary>
  ///   Creates a new instance of <see cref="BatchDispatchRequest" />.
  ///   Unique identifiers (to identifier elements within the batch) are generated for each notice.
  /// </summary>
  /// <param name="notices">A sequence of notices to dispatch.</param>
  /// <param name="batchIdProvider">
  ///   A function which generates unique identifiers for each notice within the batch.
  ///   Optional. Default: <see cref="Guid.NewGuid" />.
  /// </param>
  /// <param name="options">Optional. Batch dispatch configuration options.</param>
  /// <returns>Returns the created request.</returns>
  /// <exception cref="ArgumentException">
  ///   Thrown if identities provided by <paramref name="batchIdProvider" />
  ///   cannot be used to create a request dictionary.
  /// </exception>
  public static BatchDispatchRequest CreateFromRoutedNotices(
    IEnumerable<BatchRoutedTypedNoticeRequest> notices,
    Func<BatchRoutedTypedNoticeRequest, string>? batchIdProvider = null,
    BatchDispatchOptions? options = null
  )
  {
    return new BatchDispatchRequest
    {
      Notices = MakeDictionary(),
      BatchDispatchOptions = options
    };

    Dictionary<string, BatchRoutedTypedNoticeRequest> MakeDictionary()
    {
      try
      {
        return notices.ToDictionary(
          value => batchIdProvider?.Invoke(value) ?? Guid.NewGuid().ToString(),
          static notice => notice
        );
      }
      catch (Exception exception)
      {
        throw new ArgumentException(
          $"Failed to create a dictionary of {nameof(notices)} using keys provided by {nameof(batchIdProvider)}.",
          exception
        );
      }
    }
  }
}

/// <summary>
///   A request to dispatch a batch of notices.
/// </summary>
/// <remarks>
///   This request is intended for use with
///   <see cref="ITypedNoticeDispatcher{TEnterpriseEventBaseType}.DispatchBatchAsync" />.
/// </remarks>
public class BatchDispatchRequest<TBaseEnterpriseEvent>
{
  /// <summary>
  ///   Gets the batch dispatch options.
  /// </summary>
  public BatchDispatchOptions? BatchDispatchOptions { get; init; }

  /// <summary>
  ///   Gets the notices which are being dispatched.
  ///   Key is a unique identifier for the notice within the batch, which is used to correlate responses.
  ///   Value is the notice itself.
  /// </summary>
  public required IReadOnlyDictionary<string, TBaseEnterpriseEvent> Notices { get; init; }

  /// <summary>
  ///   Creates a new instance of <see cref="BatchDispatchRequest{TBaseEnterpriseEvent}" />.
  ///   Unique identifiers (to identify elements within the notice batch) are generated for each notice.
  ///   If <typeparamref name="TBaseEnterpriseEvent" /> extends <see cref="EnterpriseEvent" />,
  ///   then <see cref="EnterpriseEvent.Id" /> is used to identify the notice element within the batch.
  ///   Otherwise, a new <see cref="Guid" /> is generated.
  /// </summary>
  /// <param name="notices">A sequence of notices to dispatch.</param>
  /// <param name="options">Optional. Batch dispatch configuration options.</param>
  /// <returns>Returns the created request.</returns>
  public static BatchDispatchRequest<TBaseEnterpriseEvent> CreateFromTypedNotices(
    IEnumerable<TBaseEnterpriseEvent> notices,
    BatchDispatchOptions? options = null
  )
  {
    return CreateFromTypedNotices(
      notices,
      static value =>
        (value is EnterpriseEvent ee ? ee.Id : Guid.NewGuid()).ToString(),
      options
    );
  }

  /// <summary>
  ///   Creates a new instance of <see cref="BatchDispatchRequest{TBaseEnterpriseEvent}" />.
  ///   Unique identifiers (to uniquely identify the element within the batch) are provided by
  ///   <paramref name="batchIdProvider" />.
  /// </summary>
  /// <param name="notices">A sequence of notices to dispatch.</param>
  /// <param name="batchIdProvider">A function which returns a unique identifier for each notice in the batch.</param>
  /// <param name="options">Optional. Batch dispatch configuration options.</param>
  /// <returns>Returns the created request.</returns>
  /// <exception cref="ArgumentException">
  ///   Thrown if identities provided by <paramref name="batchIdProvider" />
  ///   cannot be used to create a request <typeparamref name="TBaseEnterpriseEvent" /> dictionary.
  /// </exception>
  public static BatchDispatchRequest<TBaseEnterpriseEvent> CreateFromTypedNotices(
    IEnumerable<TBaseEnterpriseEvent> notices,
    Func<TBaseEnterpriseEvent, string> batchIdProvider,
    BatchDispatchOptions? options
  )
  {
    return new BatchDispatchRequest<TBaseEnterpriseEvent>
    {
      Notices = MakeDictionary(),
      BatchDispatchOptions = options
    };

    Dictionary<string, TBaseEnterpriseEvent> MakeDictionary()
    {
      try
      {
        return notices.ToDictionary(batchIdProvider, static notice => notice);
      }
      catch (Exception exception)
      {
        throw new ArgumentException(
          $"Failed to create a dictionary of {nameof(notices)} using keys provided by {nameof(batchIdProvider)}.",
          exception
        );
      }
    }
  }
}
