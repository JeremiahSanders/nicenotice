using Jds.NiceNotice.TypedNotices.Metadata;
using Jds.NiceNotice.TypedNotices.Routing;
using Jds.NiceNotice.TypedNotices.Serialization;
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
    Either<Exception, string> possibleSerialized = Eithers.Try(() => Serialize(notice));
    if (possibleSerialized.IsLeft)
    {
      return new TypedNoticeDispatchResult<TEventType>
      {
        Notice = notice,
        Exception = possibleSerialized.LeftUnsafe,
        IoRequest = new IoNoticeDispatchRequest(streamId, string.Empty, metadata: null, contentType: null)
      };
    }

    string serialized = possibleSerialized.IfLeftThrow();
    string? serializedContentType = Eithers.Try(GetSerializerContentType).FoldRight(_ => string.Empty);

    Either<Exception, string> possibleValidated = Eithers.Try(() => Validate(notice, serialized));
    if (possibleValidated.IsLeft)
    {
      return new TypedNoticeDispatchResult<TEventType>
      {
        Notice = notice,
        Exception = possibleValidated.LeftUnsafe,
        IoRequest = new IoNoticeDispatchRequest(streamId, serialized, contentType: serializedContentType, metadata: null)
      };
    }

    string validated = possibleValidated.IfLeftThrow();

    Either<Exception, IReadOnlyDictionary<string, NoticeMetadataValue>?> possibleMetadata = Eithers.Try(() => GetMetadata(notice));
    if (possibleMetadata.IsLeft)
    {
      return new TypedNoticeDispatchResult<TEventType>
      {
        Notice = notice,
        Exception = possibleMetadata.LeftUnsafe,
        IoRequest = new IoNoticeDispatchRequest(streamId, validated, contentType: serializedContentType, metadata: null)
      };
    }

    IReadOnlyDictionary<string, NoticeMetadataValue>? metadata = possibleMetadata.IfLeftThrow();

    IoNoticeDispatchRequest ioRequest = new(streamId, validated, metadata, serializedContentType);
    IoNoticeDispatchResult response = await Dispatch(ioRequest);

    return new TypedNoticeDispatchResult<TEventType>
    {
      Notice = notice,
      IoRequest = ioRequest,
      Exception = response.Exception
    };

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

    async Task<IoNoticeDispatchResult> Dispatch(IoNoticeDispatchRequest requestNotice)
    {
      try
      {
        return await IoDispatcherProvider().DispatchAsync(requestNotice, cancellationToken);
      }
      catch (Exception exception)
      {
        return new IoNoticeDispatchResult(
          streamId,
          requestNotice.Notice,
          requestNotice.Metadata,
          requestNotice.ContentType,
          exception
        );
      }
    }
  }

  /// <inheritdoc />
  public virtual async Task<BatchTypedNoticeDispatchResult> DispatchBatchAsync(
    BatchDispatchRequest request,
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
  /// <param name="metadataProvider">
  ///   Optional. A notice metadata provider.
  ///   Defaults to <see cref="Jds.NiceNotice.TypedNotices.Metadata.MetadataProviders.DefaultMetadataProvider" />.
  /// </param>
  /// <returns></returns>
  public static TypedNoticeDispatcher Create(
    INoticeIo ioDispatcher,
    NoticeSerializer? noticeSerializer = null,
    NoticeValidator? noticeValidator = null,
    NoticeMetadataProvider? metadataProvider = null
  )
  {
    return new DefaultTypedNoticeDispatcher(
      () => ioDispatcher,
      noticeSerializer ?? Serializers.Json(),
      noticeValidator ?? Validators.NoOpValidator(),
      metadataProvider ?? MetadataProviders.DefaultMetadataProvider()
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
  /// <param name="metadataProvider">
  ///   Optional. A notice metadata provider.
  ///   Defaults to <see cref="Jds.NiceNotice.TypedNotices.Metadata.MetadataProviders.DefaultMetadataProvider" />.
  /// </param>
  /// <returns></returns>
  public static TypedNoticeDispatcher Create(
    Func<INoticeIo> ioDispatcherProvider,
    NoticeSerializer? noticeSerializer = null,
    NoticeValidator? noticeValidator = null,
    NoticeMetadataProvider? metadataProvider = null
  )
  {
    return new DefaultTypedNoticeDispatcher(
      ioDispatcherProvider,
      noticeSerializer ?? Serializers.Json(),
      noticeValidator ?? Validators.NoOpValidator(),
      metadataProvider ?? MetadataProviders.DefaultMetadataProvider()
    );
  }

  /// <summary>
  ///   Gets the content type to which <see cref="SerializeNotice{TEventType}(TEventType)" /> will serialize the notice,
  ///   e.g., <c>application/json</c>.
  /// </summary>
  /// <param name="notice"></param>
  /// <typeparam name="TEventType"></typeparam>
  /// <returns></returns>
  protected virtual IReadOnlyDictionary<string, NoticeMetadataValue>? GetMetadata<TEventType>(TEventType notice)
    where TEventType : notnull
  {
    return null;
  }

  /// <summary>
  ///   Gets the content type to which <see cref="SerializeNotice{TEventType}" /> will serialize the notice,
  ///   e.g., <c>application/json</c>
  /// </summary>
  /// <returns>Returns the content type.</returns>
  protected abstract string? GetSerializerContentType();

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

  private Either<Exception, string> TrySerialize(object toSerialize)
  {
    return Eithers
      .Try(() => SerializeNotice(toSerialize))
      .MapLeft<Exception>(e => new NoticeSerializationException(message: "Failed to serialize notice.", e));
  }

  private BatchRoutedTypedNoticeResponse TrySerializeAndValidate(
    string batchedNoticeId,
    BatchRoutedTypedNoticeRequest batchRoutedTypedNoticeRequest
  )
  {
    Either<Exception, string> possiblySerialized = TrySerialize(batchRoutedTypedNoticeRequest.Notice);

    if (possiblySerialized.IsLeft)
    {
      return new BatchRoutedTypedNoticeResponse(
        batchedNoticeId,
        batchRoutedTypedNoticeRequest.Stream,
        batchRoutedTypedNoticeRequest.Notice,
        string.Empty,
        string.Empty,
        metadata: null,
        possiblySerialized.LeftUnsafe
      );
    }

    string serialized = possiblySerialized.IfLeftThrow();

    string? serializedContentType = Eithers.Try(GetSerializerContentType).FoldRight(_ => string.Empty);

    Either<Exception, string> possiblyValidated =
      Eithers.Try(() => Validate(batchRoutedTypedNoticeRequest.Notice, serialized));
    if (possiblyValidated.IsLeft)
    {
      return new BatchRoutedTypedNoticeResponse(
        batchedNoticeId,
        batchRoutedTypedNoticeRequest.Stream,
        batchRoutedTypedNoticeRequest.Notice,
        serialized,
        serializedContentType,
        metadata: null,
        possiblyValidated.LeftUnsafe
      );
    }

    string validated = possiblyValidated.IfLeftThrow();

    Either<Exception, IReadOnlyDictionary<string, NoticeMetadataValue>?> possiblyMetadata =
      Eithers.Try(() => GetMetadata(batchRoutedTypedNoticeRequest.Notice));
    if (possiblyMetadata.IsLeft)
    {
      return new BatchRoutedTypedNoticeResponse(
        batchedNoticeId,
        batchRoutedTypedNoticeRequest.Stream,
        batchRoutedTypedNoticeRequest.Notice,
        validated,
        serializedContentType,
        metadata: null,
        possiblyMetadata.LeftUnsafe
      );
    }

    IReadOnlyDictionary<string, NoticeMetadataValue>? metadata = possiblyMetadata.IfLeftThrow();

    BatchRoutedTypedNoticeResponse response = new(
      batchedNoticeId,
      batchRoutedTypedNoticeRequest.Stream,
      batchRoutedTypedNoticeRequest.Notice,
      validated,
      serializedContentType,
      metadata,
      exception: null
    );

    return response;
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
        $"{notice.GetType().Name} validation failed.",
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
  ///   The specific type of the enterprise event being dispatched,
  ///   constrained to the base event type.
  /// </typeparam>
  /// <returns>
  ///   Returns the dispatch result.
  ///   Failures are captured and returned within <see cref="TypedNoticeDispatchResult{TEventType}.Exception" />.
  /// </returns>
  public virtual async Task<TypedNoticeDispatchResult<TEventType>> DispatchAsync<TEventType>(
    TEventType notice,
    CancellationToken cancellationToken = default
  )
    where TEventType : TEnterpriseEventBaseType
  {
    Either<Exception, EventStreamId> possibleStream = TryGetStream(notice);

    if (possibleStream.IsLeft)
    {
      return new TypedNoticeDispatchResult<TEventType>
      {
        Notice = notice,
        Exception = possibleStream.LeftUnsafe,
        IoRequest = new IoNoticeDispatchRequest(
          EventStreamId.From(string.Empty),
          string.Empty,
          metadata: null,
          contentType: null
        )
      };
    }

    EventStreamId streamId = possibleStream.IfLeftThrow();

    Either<Exception, string> possibleSerialized = TrySerialize(notice);
    if (possibleSerialized.IsLeft)
    {
      return new TypedNoticeDispatchResult<TEventType>
      {
        Notice = notice,
        Exception = possibleSerialized.LeftUnsafe,
        IoRequest = new IoNoticeDispatchRequest(streamId, string.Empty, metadata: null, contentType: null)
      };
    }

    string serializedContentType = Eithers.Try(GetSerializedContentType).FoldRight(_ => string.Empty);
    string serialized = possibleSerialized.IfLeftThrow();

    Either<Exception, string> possibleValidated = TryValidate(notice, serialized);
    if (possibleValidated.IsLeft)
    {
      return new TypedNoticeDispatchResult<TEventType>
      {
        Notice = notice,
        Exception = possibleValidated.LeftUnsafe,
        IoRequest = new IoNoticeDispatchRequest(streamId, serialized, contentType: serializedContentType, metadata: null)
      };
    }

    string validated = possibleValidated.IfLeftThrow();

    Either<Exception, IReadOnlyDictionary<string, NoticeMetadataValue>?> possibleMetadata = TryGetMetadata(notice);
    if (possibleMetadata.IsLeft)
    {
      return new TypedNoticeDispatchResult<TEventType>
      {
        Notice = notice,
        Exception = possibleMetadata.LeftUnsafe,
        IoRequest = new IoNoticeDispatchRequest(streamId, validated, contentType: serializedContentType, metadata: null)
      };
    }

    IoNoticeDispatchRequest ioRequest = new(
      streamId,
      validated,
      contentType: serializedContentType,
      metadata: GetMetadata(notice)
    );
    IoNoticeDispatchResult response = await Dispatch(ioRequest);

    return new TypedNoticeDispatchResult<TEventType>
    {
      Notice = notice,
      Exception = response.Exception,
      IoRequest = ioRequest
    };

    async Task<IoNoticeDispatchResult> Dispatch(IoNoticeDispatchRequest noticeDto)
    {
      try
      {
        return await IoDispatcherProvider().DispatchAsync(noticeDto, cancellationToken);
      }
      catch (Exception exception)
      {
        return new IoNoticeDispatchResult(
          noticeDto.Stream,
          noticeDto.Notice,
          noticeDto.Metadata,
          noticeDto.ContentType,
          new IOException(message: "Failed to dispatch enterprise event.", exception)
        );
      }
    }
  }

  /// <inheritdoc />
  public async Task<BatchTypedNoticeDispatchResult> DispatchBatchAsync<TEventType>(
    BatchDispatchRequest<TEventType> request,
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
                  new BatchRoutedTypedNoticeResponse(
                    kvp.Key,
                    EventStreamId.From(string.Empty),
                    notice,
                    string.Empty,
                    string.Empty,
                    metadata: null,
                    failure
                  ),
                  failure)
                );

            return routeResult;
          }
        )
        .ToList()
        .Partition();

    string serializedContentType = Eithers.Try(GetSerializedContentType).FoldRight(_ => string.Empty);

    BatchTypedNoticeDispatchResult results = await BatchDispatchingWorkflow.DispatchBatchAsync(
      IoDispatcherProvider,
      TrySerializeAndValidate,
      routed.rights.ToDictionary(),
      request.BatchDispatchOptions,
      cancellationToken
    );

    return results;

    BatchRoutedTypedNoticeResponse TrySerializeAndValidate(string id, BatchRoutedTypedNoticeRequest notice)
    {
      Either<Exception, string> possiblySerialized = TrySerialize((TEnterpriseEventBaseType)notice.Notice);
      if (possiblySerialized.IsLeft)
      {
        return new BatchRoutedTypedNoticeResponse(
          id,
          notice.Stream,
          notice.Notice,
          string.Empty,
          string.Empty,
          metadata: null,
          possiblySerialized.LeftUnsafe
        );
      }

      string serialized = possiblySerialized.IfLeftThrow();
      Either<Exception, string> possiblyValidated = TryValidate((TEnterpriseEventBaseType)notice.Notice, serialized);
      if (possiblyValidated.IsLeft)
      {
        return new BatchRoutedTypedNoticeResponse(
          id,
          notice.Stream,
          notice.Notice,
          serialized,
          serializedContentType,
          metadata: null,
          possiblyValidated.LeftUnsafe
        );
      }

      string validated = possiblyValidated.IfLeftThrow();

      Either<Exception, IReadOnlyDictionary<string, NoticeMetadataValue>?> possiblyMetadata =
        TryGetMetadata((TEnterpriseEventBaseType)notice.Notice);
      if (possiblyMetadata.IsLeft)
      {
        return new BatchRoutedTypedNoticeResponse(
          id,
          notice.Stream,
          notice.Notice,
          validated,
          serializedContentType,
          metadata: null,
          possiblyMetadata.LeftUnsafe
        );
      }

      IReadOnlyDictionary<string, NoticeMetadataValue>? metadata = possiblyMetadata.IfLeftThrow();

      return new BatchRoutedTypedNoticeResponse(
        id,
        notice.Stream,
        notice.Notice,
        validated,
        serializedContentType,
        metadata,
        exception: null
      );
    }
  }

  /// <summary>
  ///   Creates an instance of <see cref="TypedNoticeDispatcher{TEnterpriseEventBaseType}" /> with the specified
  ///   dispatcher, stream selector, and notice serializer functions.
  /// </summary>
  /// <param name="ioDispatcher">The dispatcher responsible for sending enterprise events to specific event streams.</param>
  /// <param name="streamSelector">A function to determine the <see cref="EventStreamId" /> for a given enterprise event.</param>
  /// <param name="noticeSerializer">A function to serialize the enterprise event into a string.</param>
  /// <param name="validateNotice">A function to identify any reasons the notice should not be dispatched.</param>
  /// <param name="metadataProvider">Optional. A metadata provider for notices.</param>
  /// <returns>A new instance of <see cref="TypedNoticeDispatcher{TEnterpriseEventBaseType}" />.</returns>
  public static TypedNoticeDispatcher<TEnterpriseEventBaseType> Create(
    INoticeIo ioDispatcher,
    NoticeRouter<TEnterpriseEventBaseType>? streamSelector = null,
    NoticeSerializer<TEnterpriseEventBaseType>? noticeSerializer = null,
    NoticeValidator<TEnterpriseEventBaseType>? validateNotice = null,
    NoticeMetadataProvider<TEnterpriseEventBaseType>? metadataProvider = null
  )
  {
    return new DefaultTypedNoticeDispatcher<TEnterpriseEventBaseType>(
      () => ioDispatcher,
      streamSelector ?? Routers.TypeNameStreams<TEnterpriseEventBaseType>(),
      noticeSerializer ?? Serializers.Json<TEnterpriseEventBaseType>(),
      validateNotice ?? Validators.NoOpValidator<TEnterpriseEventBaseType>(),
      metadataProvider ?? MetadataProviders.DefaultMetadataProvider<TEnterpriseEventBaseType>()
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
  /// <param name="metadataProvider">Optional. A metadata provider for notices.</param>
  /// <returns>A new instance of <see cref="TypedNoticeDispatcher{TEnterpriseEventBaseType}" />.</returns>
  public static TypedNoticeDispatcher<TEnterpriseEventBaseType> Create(
    Func<INoticeIo> ioDispatcherProvider,
    NoticeRouter<TEnterpriseEventBaseType>? streamSelector = null,
    NoticeSerializer<TEnterpriseEventBaseType>? noticeSerializer = null,
    NoticeValidator<TEnterpriseEventBaseType>? validateNotice = null,
    NoticeMetadataProvider<TEnterpriseEventBaseType>? metadataProvider = null
  )
  {
    return new DefaultTypedNoticeDispatcher<TEnterpriseEventBaseType>(
      ioDispatcherProvider,
      streamSelector ?? Routers.TypeNameStreams<TEnterpriseEventBaseType>(),
      noticeSerializer ?? Serializers.Json<TEnterpriseEventBaseType>(),
      validateNotice ?? Validators.NoOpValidator<TEnterpriseEventBaseType>(),
      metadataProvider ?? MetadataProviders.DefaultMetadataProvider<TEnterpriseEventBaseType>()
    );
  }

  /// <summary>
  ///   Gets the metadata to be attached to the notice when dispatching to I/O.
  /// </summary>
  /// <param name="notice">The enterprise event notice which is being dispatched.</param>
  /// <returns>Returns the metadata dictionary.</returns>
  protected virtual IReadOnlyDictionary<string, NoticeMetadataValue>? GetMetadata<TEnterpriseEvent>(TEnterpriseEvent notice)
    where TEnterpriseEvent : TEnterpriseEventBaseType
  {
    return null;
  }

  /// <summary>
  ///   Gets the content type to which <see cref="SerializeNotice{TEventType}(TEventType)" /> will serialize the notice,
  ///   e.g., <c>application/json</c>
  /// </summary>
  /// <returns>Returns the content type.</returns>
  protected abstract string GetSerializedContentType();

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

  private Either<Exception, IReadOnlyDictionary<string, NoticeMetadataValue>?> TryGetMetadata<TEventType>(TEventType notice)
    where TEventType : TEnterpriseEventBaseType
  {
    Either<Exception, IReadOnlyDictionary<string, NoticeMetadataValue>?> possibleMetadata = Eithers.Try(() => GetMetadata(notice));

    Either<Exception, IReadOnlyDictionary<string, NoticeMetadataValue>?> mappedLeft =
      possibleMetadata.MapLeft(Exception (exception) =>
        new IOException(message: "Failed to get notice metadata.", exception)
      );

    return mappedLeft;
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
            $"{notice.GetType().Name} validation failed.",
            validationResults,
            innerException: null
          );
        }

        return serializedNotice;
      }
    );
  }
}
