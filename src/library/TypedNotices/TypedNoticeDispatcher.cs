using Jds.NiceNotice.TypedNotices.Routing;
using Jds.NiceNotice.TypedNotices.Serialization;
using Jds.NiceNotice.TypedNotices.Serialization.Implementations;
using Jds.NiceNotice.TypedNotices.Validation;
using Jds.NiceNotice.TypedNotices.Validation.Implementations;

namespace Jds.NiceNotice.TypedNotices;

/// <summary>
///   A base class implementation of <see cref="ITypedNoticeDispatcher" />.
///   Provides <c>abstract</c> and <c>virtual</c> methods for customizing its behavior.
/// </summary>
/// <param name="ioDispatcherProvider">
///   A function which will return a notice I/O implementation.
///   This function will be invoked each time a notice is dispatched.
/// </param>
public abstract class TypedNoticeDispatcher(Func<INoticeIo> ioDispatcherProvider)
  : ITypedNoticeDispatcher
{
  /// <summary>
  ///   Gets the function that provides a notification dispatcher,
  ///   which is responsible for sending serialized enterprise events to specific event streams.
  /// </summary>
  protected Func<INoticeIo> IoDispatcherProvider { get; } = ioDispatcherProvider;

  /// <inheritdoc />
  public virtual async Task<TypedNoticeDispatchResult<TEventType>> DispatchAsync<TEventType>(
    TEventType notice,
    EventStreamId streamId,
    CancellationToken cancellationToken = default) where TEventType : notnull
  {
    RoutedTypedNotice routedNotice = RouteAndValidate(notice, streamId);
    string response = await Dispatch(streamId, routedNotice.SerializedNotice);

    return new TypedNoticeDispatchResult<TEventType>
    {
      Notice = notice,
      Serialized = routedNotice.SerializedNotice,
      IoResponse = response,
      Stream = streamId
    };

    async Task<string> Dispatch(EventStreamId stream, string serializedMessage)
    {
      try
      {
        return await IoDispatcherProvider().DispatchAsync(stream, serializedMessage, cancellationToken);
      }
      catch (Exception e)
      {
        throw new IOException(message: "Failed to dispatch notice.", e);
      }
    }
  }

  /// <inheritdoc />
  public virtual async Task<BatchTypedNoticeDispatchResult> DispatchBatchAsync(
    DispatchBatchRequest request,
    CancellationToken cancellationToken = default)
  {
    return await BatchDispatchingWorkflow.DispatchBatchAsync(
      IoDispatcherProvider,
      TrySerializeAndValidate,
      request.Notices,
      request.BatchDispatchOptions,
      cancellationToken
    );
  }

  /// <summary>
  ///   Creates a typed notice dispatcher using the specified I/O dispatcher, notice serializer, and notice validator.
  /// </summary>
  /// <param name="ioDispatcher">A notice I/O implementation.</param>
  /// <param name="noticeSerializer">Optional. A notice serializer. Defaults to <c>json</c> serialization.</param>
  /// <param name="noticeValidator">
  ///   Optional. A notice serializer.
  ///   Defaults to <see cref="NoOpNoticeValidator" /> (i.e., no validation is performed).
  ///   Create an instance with <see cref="Validators.DataAnnotationsValidator" />
  ///   to use standard data annotation validation.
  /// </param>
  /// <returns></returns>
  public static TypedNoticeDispatcher Create(
    INoticeIo ioDispatcher,
    NoticeSerializer? noticeSerializer = null,
    NoticeValidator? noticeValidator = null)
  {
    return new DefaultTypedNoticeDispatcher(
      () => ioDispatcher,
      noticeSerializer ?? new JsonNoticeSerializer(),
      noticeValidator ?? new NoOpNoticeValidator()
    );
  }

  /// <summary>
  ///   Creates a typed notice dispatcher using the specified I/O dispatcher, notice serializer, and notice validator.
  /// </summary>
  /// <param name="ioDispatcherProvider">
  ///   A function that returns an I/O dispatcher which is responsible
  ///   for sending enterprise events to specific event streams.
  /// </param>
  /// <param name="noticeSerializer">Optional. A notice serializer. Defaults to <c>json</c> serialization.</param>
  /// <param name="noticeValidator">
  ///   Optional. A notice serializer.
  ///   Defaults to <see cref="NoOpNoticeValidator" /> (i.e., no validation is performed).
  ///   Create an instance with <see cref="Validators.DataAnnotationsValidator" />
  ///   to use standard data annotation validation.
  /// </param>
  /// <returns></returns>
  public static TypedNoticeDispatcher Create(
    Func<INoticeIo> ioDispatcherProvider,
    NoticeSerializer? noticeSerializer = null,
    NoticeValidator? noticeValidator = null)
  {
    return new DefaultTypedNoticeDispatcher(
      ioDispatcherProvider,
      noticeSerializer ?? new JsonNoticeSerializer(),
      noticeValidator ?? new NoOpNoticeValidator()
    );
  }

  /// <summary>
  ///   Serializes the specified enterprise event notice to a string representation.
  /// </summary>
  /// <param name="notice">The enterprise event notice.</param>
  /// <typeparam name="TEventType">The enterprise event notice type.</typeparam>
  /// <returns></returns>
  protected abstract string SerializeNotice<TEventType>(TEventType notice) where TEventType : notnull;

  /// <summary>
  ///   Validates the specified enterprise event notice.
  /// </summary>
  /// <param name="notice">The notice being dispatched.</param>
  /// <param name="serializedNotice">
  ///   The serialized <paramref name="notice" />.
  ///   Useful for performing I/O-related validation.
  ///   For example, rejecting notices whose serialized version exceeds API limits for the final enterprise event bus.
  ///   (E.g., AWS SNS has a maximum allowed notification message length.)
  /// </param>
  /// <typeparam name="TEventType">The enterprise event notice type.</typeparam>
  /// <returns></returns>
  protected virtual IReadOnlyList<string>? ValidateNotice<TEventType>(TEventType notice, string serializedNotice)
    where TEventType : notnull
  {
    return null;
  }

  private RoutedTypedNotice RouteAndValidate(object notice, EventStreamId streamId)
  {
    string serialized = Serialize(notice);

    return new RoutedTypedNotice(streamId, notice, Validate(notice, serialized));

    string Serialize(object toSerialize)
    {
      try
      {
        return SerializeNotice(toSerialize);
      }
      catch (Exception e)
      {
        throw new NoticeSerializationException(message: "Failed to serialize notice.", e);
      }
    }
  }

  private Either<Exception, string> TrySerialize(object toSerialize)
  {
    return Eithers
      .Try(() => SerializeNotice(toSerialize))
      .MapLeft<Exception>(e => new NoticeSerializationException(message: "Failed to serialize notice.", e));
  }

  private Either<(BatchRoutedTypedNoticeResponse, Exception), BatchRoutedTypedNoticeResponse> TrySerializeAndValidate(
    string batchedNoticeId,
    BatchRoutedTypedNoticeRequest batchRoutedTypedNoticeRequest)
  {
    Either<Exception, string> serialized = TrySerialize(batchRoutedTypedNoticeRequest.Notice);
    Either<Exception, string> validated =
      serialized.Map(serializedNotice => Validate(batchRoutedTypedNoticeRequest.Notice, serializedNotice));
    Either<Exception, BatchRoutedTypedNoticeResponse> rtn =
      validated.Map(validatedNotice => new BatchRoutedTypedNoticeResponse(
          batchedNoticeId,
          batchRoutedTypedNoticeRequest.Stream,
          batchRoutedTypedNoticeRequest.Notice,
          validatedNotice
        )
      );

    if (rtn.IsRight)
    {
      return Eithers
        .Right<(BatchRoutedTypedNoticeResponse, Exception), BatchRoutedTypedNoticeResponse>(rtn.RightUnsafe);
    }

    // We failed some portion
    Exception failure = rtn.LeftUnsafe;
    string serializedOrEmpty = serialized.FoldRight(_ => string.Empty);

    return Eithers.Left<(BatchRoutedTypedNoticeResponse, Exception), BatchRoutedTypedNoticeResponse>(
      (new BatchRoutedTypedNoticeResponse(batchedNoticeId, batchRoutedTypedNoticeRequest.Stream, batchRoutedTypedNoticeRequest.Notice, serializedOrEmpty),
        failure)
    );
  }

  private string Validate<TEvent>(TEvent notice, string noticeJson)
    where TEvent : notnull
  {
    IReadOnlyList<string>? validationResults;
    try
    {
      validationResults = ValidateNotice(notice, noticeJson);
    }
    catch (NoticeValidationException)
    {
      throw;
    }
    catch (Exception e)
    {
      throw new NoticeValidationException(message: "Failed to validate typed notice.", e);
    }

    if (validationResults is {Count: > 0})
    {
      throw new NoticeValidationException(
        $"{typeof(TEvent).Name} validation failed.",
        validationResults,
        innerException: null
      );
    }

    return noticeJson;
  }

  private record RoutedTypedNotice(EventStreamId Stream, object Notice, string SerializedNotice);
}

/// <summary>
///   Represents an abstract base class for dispatching notifications of a specified type.
/// </summary>
/// <typeparam name="TEnterpriseEventBaseType">
///   The base type of enterprise events that the dispatcher handles. Must be a non-nullable type.
/// </typeparam>
/// <param name="ioDispatcherProvider">
///   A function which will return a notice I/O implementation.
///   This function will be invoked each time a notice is dispatched.
/// </param>
public abstract class TypedNoticeDispatcher<TEnterpriseEventBaseType>(Func<INoticeIo> ioDispatcherProvider)
  : ITypedNoticeDispatcher<TEnterpriseEventBaseType>
  where TEnterpriseEventBaseType : notnull
{
  /// <summary>
  ///   Gets the function that provides a notification dispatcher,
  ///   which is responsible for sending serialized enterprise events to specific event streams.
  /// </summary>
  protected Func<INoticeIo> IoDispatcherProvider { get; } = ioDispatcherProvider;

  /// <summary>
  ///   Dispatches an enterprise event asynchronously after performing validation, stream determination, serialization,
  ///   and dispatch through the configured notification dispatcher.
  /// </summary>
  /// <param name="notice">The enterprise event to be validated, serialized, and dispatched.</param>
  /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
  /// <typeparam name="TEventType">
  ///   The specific type of the enterprise event being dispatched, constrained to the base event
  ///   type.
  /// </typeparam>
  /// <returns>The dispatched enterprise event.</returns>
  /// <exception cref="NoticeValidationException">Thrown if the event fails validation.</exception>
  /// <exception cref="NoticeSerializationException">Thrown if the event fails serialization.</exception>
  /// <exception cref="NoticeRoutingException">Thrown if the event stream ID cannot be determined.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during dispatch.</exception>
  public virtual async Task<TypedNoticeDispatchResult<TEventType>> DispatchAsync<TEventType>(
    TEventType notice,
    CancellationToken cancellationToken = default
  )
    where TEventType : TEnterpriseEventBaseType
  {
    EventStreamId streamId = TryGetStream(notice).IfLeftThrow();

    string noticeJson = TrySerialize(notice)
      .Bind(serialized => TryValidate(notice, serialized))
      .IfLeftThrow();
    string response = await Dispatch(streamId, noticeJson);

    return new TypedNoticeDispatchResult<TEventType>
    {
      Notice = notice,
      Serialized = noticeJson,
      IoResponse = response,
      Stream = streamId
    };

    async Task<string> Dispatch(EventStreamId stream, string serializedMessage)
    {
      try
      {
        return await IoDispatcherProvider().DispatchAsync(stream, serializedMessage, cancellationToken);
      }
      catch (Exception e)
      {
        throw new IOException(message: "Failed to dispatch enterprise event.", e);
      }
    }
  }

  /// <inheritdoc />
  public async Task<BatchTypedNoticeDispatchResult> DispatchBatchAsync<TEventType>(
    DispatchBatchRequest<TEventType> request,
    CancellationToken cancellationToken = default) where TEventType : TEnterpriseEventBaseType
  {
    (List<(BatchRoutedTypedNoticeResponse, Exception failure)> lefts,
      List<KeyValuePair<string, BatchRoutedTypedNoticeRequest>> rights) routed = request
        .Notices
        .Select(kvp =>
          {
            TEnterpriseEventBaseType notice = kvp.Value;
            Either<(BatchRoutedTypedNoticeResponse, Exception failure),
                KeyValuePair<string, BatchRoutedTypedNoticeRequest>>
              routeResult = Eithers
                .Try(() => new KeyValuePair<string, BatchRoutedTypedNoticeRequest>(
                    kvp.Key,
                    new BatchRoutedTypedNoticeRequest(GetStreamId(notice), notice)
                  )
                )
                .MapLeft(failure => (
                  new BatchRoutedTypedNoticeResponse(kvp.Key, EventStreamId.From(string.Empty), notice, string.Empty),
                  failure)
                );

            return routeResult;
          }
        )
        .ToList()
        .Partition();

    BatchTypedNoticeDispatchResult results = await BatchDispatchingWorkflow.DispatchBatchAsync(
      IoDispatcherProvider,
      Either<(BatchRoutedTypedNoticeResponse, Exception), BatchRoutedTypedNoticeResponse> (id, notice) =>
        TrySerialize((TEnterpriseEventBaseType)notice.Notice)
          .BiBind(
            serialized => TryValidate((TEnterpriseEventBaseType)notice.Notice, serialized)
              .Map(validated => new BatchRoutedTypedNoticeResponse(id, notice.Stream, notice.Notice, validated))
              .MapLeft(exception => (new BatchRoutedTypedNoticeResponse(id, notice.Stream, notice.Notice, serialized),
                exception)
              ),
            exception =>
              Eithers.Left<(BatchRoutedTypedNoticeResponse, Exception exception), BatchRoutedTypedNoticeResponse>(
                (new BatchRoutedTypedNoticeResponse(id, notice.Stream, notice.Notice, string.Empty), exception)
              )
          ),
      routed.rights.ToDictionary(),
      request.BatchDispatchOptions,
      cancellationToken
    );

    return results;
  }

  /// <summary>
  ///   Creates an instance of <see cref="TypedNoticeDispatcher{TEnterpriseEventBaseType}" /> with the specified
  ///   dispatcher, stream selector, and notice serializer functions.
  /// </summary>
  /// <param name="ioDispatcher">The dispatcher responsible for sending enterprise events to specific event streams.</param>
  /// <param name="streamSelector">A function to determine the <see cref="EventStreamId" /> for a given enterprise event.</param>
  /// <param name="noticeSerializer">A function to serialize the enterprise event into a string.</param>
  /// <param name="validateNotice">A function to identify any reasons the notice should not be dispatched.</param>
  /// <returns>A new instance of <see cref="TypedNoticeDispatcher{TEnterpriseEventBaseType}" />.</returns>
  public static TypedNoticeDispatcher<TEnterpriseEventBaseType> Create(
    INoticeIo ioDispatcher,
    NoticeRouter<TEnterpriseEventBaseType>? streamSelector = null,
    NoticeSerializer<TEnterpriseEventBaseType>? noticeSerializer = null,
    NoticeValidator<TEnterpriseEventBaseType>? validateNotice = null
  )
  {
    return new DefaultTypedNoticeDispatcher<TEnterpriseEventBaseType>(
      () => ioDispatcher,
      streamSelector ?? Routers.TypeNameStreams<TEnterpriseEventBaseType>(),
      noticeSerializer ?? new JsonNoticeSerializer<TEnterpriseEventBaseType>(),
      validateNotice ?? new NoOpNoticeValidator<TEnterpriseEventBaseType>()
    );
  }

  /// <summary>
  ///   Creates an instance of <see cref="TypedNoticeDispatcher{TEnterpriseEventBaseType}" /> with the specified
  ///   dispatcher, stream selector, and notice serializer functions.
  /// </summary>
  /// <param name="ioDispatcherProvider">
  ///   A function that returns an I/O dispatcher which is responsible
  ///   for sending enterprise events to specific event streams.
  /// </param>
  /// <param name="streamSelector">A function to determine the <see cref="EventStreamId" /> for a given enterprise event.</param>
  /// <param name="noticeSerializer">A function to serialize the enterprise event into a string.</param>
  /// <param name="validateNotice">A function to identify any reasons the notice should not be dispatched.</param>
  /// <returns>A new instance of <see cref="TypedNoticeDispatcher{TEnterpriseEventBaseType}" />.</returns>
  public static TypedNoticeDispatcher<TEnterpriseEventBaseType> Create(
    Func<INoticeIo> ioDispatcherProvider,
    NoticeRouter<TEnterpriseEventBaseType>? streamSelector = null,
    NoticeSerializer<TEnterpriseEventBaseType>? noticeSerializer = null,
    NoticeValidator<TEnterpriseEventBaseType>? validateNotice = null
  )
  {
    return new DefaultTypedNoticeDispatcher<TEnterpriseEventBaseType>(
      ioDispatcherProvider,
      streamSelector ?? Routers.TypeNameStreams<TEnterpriseEventBaseType>(),
      noticeSerializer ?? new JsonNoticeSerializer<TEnterpriseEventBaseType>(),
      validateNotice ?? new NoOpNoticeValidator<TEnterpriseEventBaseType>()
    );
  }

  /// <summary>
  ///   Gets the event stream ID for the specified enterprise event.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Implementations of this method often either return a constant value (if all notices go to the same stream)
  ///     or are determined by switching upon the notice's type.
  ///   </para>
  /// </remarks>
  /// <param name="notice">An enterprise event notice being dispatched.</param>
  /// <returns>Returns the event stream ID.</returns>
  protected abstract EventStreamId GetStreamId(TEnterpriseEventBaseType notice);

  /// <summary>
  ///   Serializes the specified enterprise event notice to a string representation.
  /// </summary>
  /// <param name="notice">The enterprise event notice.</param>
  /// <typeparam name="TEventType">The enterprise event notice type.</typeparam>
  /// <returns></returns>
  protected abstract string SerializeNotice<TEventType>(TEventType notice) where TEventType : TEnterpriseEventBaseType;

  /// <summary>
  ///   Validates the specified enterprise event notice.
  /// </summary>
  /// <param name="notice">The notice being dispatched.</param>
  /// <param name="serializedNotice">
  ///   The serialized <paramref name="notice" />.
  ///   Useful for performing I/O-related validation.
  ///   For example, rejecting notices whose serialized version exceeds API limits for the final enterprise event bus.
  ///   (E.g., AWS SNS has a maximum allowed notification message length.)
  /// </param>
  /// <typeparam name="TEventType">The enterprise event notice type.</typeparam>
  /// <returns></returns>
  protected virtual IReadOnlyList<string>? ValidateNotice<TEventType>(TEventType notice, string serializedNotice)
    where TEventType : TEnterpriseEventBaseType
  {
    return null;
  }

  private Either<Exception, EventStreamId> TryGetStream<TEventType>(TEventType notice)
    where TEventType : TEnterpriseEventBaseType
  {
    return Eithers.Try(() =>
      {
        try
        {
          return GetStreamId(notice);
        }
        catch (Exception e)
        {
          throw new NoticeRoutingException(message: "Failed to determine event stream.", e);
        }
      }
    );
  }

  private Either<Exception, string> TrySerialize(TEnterpriseEventBaseType toSerialize)
  {
    return Eithers
      .Try(() => SerializeNotice(toSerialize))
      .MapLeft(Exception (e) => new NoticeSerializationException(message: "Failed to serialize enterprise event.", e));
  }

  private Either<Exception, string> TryValidate<TEventType>(TEventType notice, string serializedNotice)
    where TEventType : TEnterpriseEventBaseType
  {
    return Eithers.Try(() =>
      {
        IReadOnlyList<string>? validationResults;
        try
        {
          validationResults = ValidateNotice(notice, serializedNotice);
        }
        catch (NoticeValidationException)
        {
          throw;
        }
        catch (Exception e)
        {
          throw new NoticeValidationException(message: "Failed to validate enterprise event.", e);
        }

        if (validationResults is {Count: > 0})
        {
          throw new NoticeValidationException(
            $"{typeof(TEventType).Name} validation failed.",
            validationResults,
            innerException: null
          );
        }

        return serializedNotice;
      }
    );
  }
}
