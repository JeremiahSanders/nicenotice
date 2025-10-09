namespace Jds.NiceNotice;

public abstract class TypedNoticeDispatcher(INoticeIo ioDispatcher)
  : ITypedNoticeDispatcher
{
  /// <summary>
  ///   Gets the notification dispatcher responsible for sending enterprise events to specific event streams.
  /// </summary>
  protected INoticeIo IoDispatcher { get; } = ioDispatcher;

  /// <inheritdoc />
  public virtual async Task<TypedNoticeDispatchResult<TEventType>> DispatchAsync<TEventType>(
    TEventType notice,
    EventStreamId streamId,
    CancellationToken cancellationToken = default) where TEventType : notnull
  {
    string serialized = Serialize(notice);
    string response = await Dispatch(streamId, Validate(notice, serialized));

    return new TypedNoticeDispatchResult<TEventType>
    {
      Notice = notice,
      Serialized = serialized,
      IoResponse = response,
      Stream = streamId
    };

    string Serialize(TEventType toSerialize)
    {
      try
      {
        return SerializeNotice(toSerialize);
      }
      catch (Exception e)
      {
        throw new SerializationException(message: "Failed to serialize notice.", e);
      }
    }

    async Task<string> Dispatch(EventStreamId stream, string serializedMessage)
    {
      try
      {
        return await IoDispatcher.DispatchAsync(stream, serializedMessage, cancellationToken);
      }
      catch (Exception e)
      {
        throw new IOException(message: "Failed to dispatch notice.", e);
      }
    }
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

  public static TypedNoticeDispatcher Create(
    INoticeIo ioDispatcher,
    NoticeSerializer? noticeSerializer = null,
    NoticeValidator? noticeValidator = null)
  {
    return new DefaultTypedNoticeDispatcher(
      ioDispatcher,
      noticeSerializer ?? new JsonNoticeSerializer(),
      noticeValidator ?? new NoOpNoticeValidator()
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
}

/// <summary>
///   Represents an abstract base class for dispatching notifications of a specified type.
/// </summary>
/// <typeparam name="TEnterpriseEventBaseType">
///   The base type of enterprise events that the dispatcher handles. Must be a non-nullable type.
/// </typeparam>
public abstract class TypedNoticeDispatcher<TEnterpriseEventBaseType>(INoticeIo ioDispatcher)
  : ITypedNoticeDispatcher<TEnterpriseEventBaseType>
  where TEnterpriseEventBaseType : notnull
{
  /// <summary>
  ///   Gets the notification dispatcher responsible for sending enterprise events to specific event streams.
  /// </summary>
  protected INoticeIo IoDispatcher { get; } = ioDispatcher;

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
  /// <exception cref="SerializationException">Thrown if the event fails serialization.</exception>
  /// <exception cref="StreamDeterminationException">Thrown if the event stream ID cannot be determined.</exception>
  /// <exception cref="IOException">Thrown if an I/O error occurs during dispatch.</exception>
  public virtual async Task<TypedNoticeDispatchResult<TEventType>> DispatchAsync<TEventType>(
    TEventType notice,
    CancellationToken cancellationToken = default
  )
    where TEventType : TEnterpriseEventBaseType
  {
    EventStreamId streamId = GetStream();
    string noticeJson = Serialize(notice);
    string response = await Dispatch(streamId, Validate());

    return new TypedNoticeDispatchResult<TEventType>
    {
      Notice = notice,
      Serialized = noticeJson,
      IoResponse = response,
      Stream = streamId
    };

    EventStreamId GetStream()
    {
      try
      {
        return GetStreamId(notice);
      }
      catch (Exception e)
      {
        throw new StreamDeterminationException(message: "Failed to determine event stream.", e);
      }
    }

    string Serialize(TEventType toSerialize)
    {
      try
      {
        return SerializeNotice(toSerialize);
      }
      catch (Exception e)
      {
        throw new SerializationException(message: "Failed to serialize enterprise event.", e);
      }
    }

    async Task<string> Dispatch(EventStreamId stream, string serializedMessage)
    {
      try
      {
        return await IoDispatcher.DispatchAsync(stream, serializedMessage, cancellationToken);
      }
      catch (Exception e)
      {
        throw new IOException(message: "Failed to dispatch enterprise event.", e);
      }
    }

    string Validate()
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

      return noticeJson;
    }
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
    NoticeStreamSelector<TEnterpriseEventBaseType>? streamSelector = null,
    NoticeSerializer<TEnterpriseEventBaseType>? noticeSerializer = null,
    NoticeValidator<TEnterpriseEventBaseType>? validateNotice = null
  )
  {
    return new DefaultTypedNoticeDispatcher<TEnterpriseEventBaseType>(
      ioDispatcher,
      streamSelector ?? StreamSelectors.TypeNameStreams<TEnterpriseEventBaseType>(),
      noticeSerializer ?? new JsonNoticeSerializer<TEnterpriseEventBaseType>(),
      validateNotice ?? new NoOpNoticeValidator<TEnterpriseEventBaseType>()
    );
  }
}
