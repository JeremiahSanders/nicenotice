using System.Net.Mime;
using System.Text.Json;

using Jds.NiceNotice.TypedNotices;
using Jds.NiceNotice.TypedNotices.Serialization;

namespace Jds.NiceNotice;

/// <summary>
///   Provides extension methods for the <see cref="INoticeIo" /> interface to dispatch notifications
///   serialized as JSON.
/// </summary>
public static class NoticeIoJsonExtensions
{
  /// <summary>
  ///   Dispatches a serialized JSON notification to the specified event stream asynchronously.
  /// </summary>
  /// <remarks>This overload uses <see cref="JsonDefaults.DefaultJsonSerializerOptions" />.</remarks>
  /// <typeparam name="TNotification">The type of the notification to be dispatched.</typeparam>
  /// <param name="dispatcher">The event dispatcher responsible for sending the notification.</param>
  /// <param name="stream">The event stream ID where the serialized JSON notification will be dispatched.</param>
  /// <param name="notice">The notification object to be serialized and dispatched.</param>
  /// <param name="metadata">Optional metadata to pass into the event I/O.</param>
  /// <param name="cancellationToken">An asynchronous operation cancellation token.</param>
  /// <returns>
  ///   A task representing the asynchronous operation, containing a tuple with the original
  ///   <typeparamref name="TNotification" />
  ///   and the serialized JSON string dispatched.
  /// </returns>
  public static Task<TypedNoticeDispatchResult<TNotification>> DispatchJsonAsync<TNotification>(
    this INoticeIo dispatcher,
    EventStreamId stream,
    TNotification notice,
    IReadOnlyDictionary<string, string>? metadata = null,
    CancellationToken cancellationToken = default
  )
    where TNotification : notnull
  {
    return dispatcher.DispatchJsonAsync(
      stream,
      notice,
      JsonDefaults.DefaultJsonSerializerOptions,
      metadata,
      cancellationToken
    );
  }

  /// <summary>
  ///   Dispatches a serialized JSON notification to the specified event stream asynchronously.
  /// </summary>
  /// <typeparam name="TNotification">The type of the notification to be dispatched.</typeparam>
  /// <param name="dispatcher">The event dispatcher responsible for sending the notification.</param>
  /// <param name="stream">The event stream ID where the serialized JSON notification will be dispatched.</param>
  /// <param name="notice">The notification object to be serialized and dispatched.</param>
  /// <param name="options">JSON serializer settings used during the serialization of the notification.</param>
  /// <param name="metadata">Optional metadata to pass into the event I/O.</param>
  /// <param name="cancellationToken">An asynchronous operation cancellation token.</param>
  /// <returns>
  ///   A task representing the asynchronous operation, containing a tuple with the original
  ///   <typeparamref name="TNotification" />
  ///   and the serialized JSON string dispatched.
  /// </returns>
  public static async Task<TypedNoticeDispatchResult<TNotification>> DispatchJsonAsync<TNotification>(
    this INoticeIo dispatcher,
    EventStreamId stream,
    TNotification notice,
    JsonSerializerOptions? options,
    IReadOnlyDictionary<string, string>? metadata,
    CancellationToken cancellationToken = default
  )
    where TNotification : notnull
  {
    Either<Exception, string> serializationResult = Eithers.Try(() => JsonSerializer.Serialize(notice, options));

    if (serializationResult.IsLeft)
    {
      return new TypedNoticeDispatchResult<TNotification>
      {
        Notice = notice,
        Exception = new NoticeSerializationException(
          message: "Failed to serialize notice.",
          serializationResult.LeftUnsafe
        ),
        IoRequest = new IoNoticeDispatchRequest(stream, string.Empty, metadata, contentType: null)
      };
    }

    Either<Exception, IoNoticeDispatchRequest> requestResult = serializationResult.Map(json =>
      new IoNoticeDispatchRequest(stream, json, contentType: MediaTypeNames.Application.Json, metadata: metadata)
    );
    if (requestResult.IsLeft)
    {
      // This shouldn't happen... but we'll add support just in case.
      return new TypedNoticeDispatchResult<TNotification>
      {
        Notice = notice,
        Exception = new IOException(message: "Failed to create I/O request.", requestResult.LeftUnsafe),
        IoRequest = new IoNoticeDispatchRequest(
          stream,
          serializationResult.RightUnsafe,
          contentType: MediaTypeNames.Application.Json,
          metadata: metadata
        )
      };
    }

    IoNoticeDispatchRequest ioRequest = requestResult.IfLeftThrow();

    Either<Exception, IoNoticeDispatchResult> dispatchedResult = await requestResult.BindAsync(request =>
      Eithers.TryAsync(() => dispatcher.DispatchAsync(request, cancellationToken))
    );

    return dispatchedResult
      .Map(result => new TypedNoticeDispatchResult<TNotification>
        {
          Notice = notice,
          IoRequest = ioRequest,
          Exception = result.Exception
        }
      )
      .FoldRight(exception => new TypedNoticeDispatchResult<TNotification>
        {
          Notice = notice,
          IoRequest = ioRequest,
          Exception = exception
        }
      );
  }
}
