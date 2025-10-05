using System.Text.Json;

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
  /// <typeparam name="TNotification">The type of the notification to be dispatched.</typeparam>
  /// <param name="dispatcher">The event dispatcher responsible for sending the notification.</param>
  /// <param name="stream">The event stream ID where the serialized JSON notification will be dispatched.</param>
  /// <param name="notice">The notification object to be serialized and dispatched.</param>
  /// <param name="options">JSON serializer settings used during the serialization of the notification.</param>
  /// <param name="cancellationToken">An asynchronous operation cancellation token.</param>
  /// <returns>
  ///   A task representing the asynchronous operation, containing a tuple with the original
  ///   <typeparamref name="TNotification" />
  ///   and the serialized JSON string dispatched.
  /// </returns>
  public static async Task<(TNotification, string)> DispatchJsonAsync<TNotification>(
    this INoticeIo dispatcher,
    EventStreamId stream,
    TNotification notice,
    JsonSerializerOptions? options,
    CancellationToken cancellationToken = default)
  {
    string json = JsonSerializer.Serialize(notice, options);
    _ = await dispatcher.DispatchAsync(stream, json, cancellationToken);

    return (notice, json);
  }

  /// <summary>
  ///   Dispatches a serialized JSON notification to the specified event stream asynchronously.
  /// </summary>
  /// <remarks>This overload uses <see cref="JsonDefaults.DefaultJsonSerializerOptions" />.</remarks>
  /// <typeparam name="TNotification">The type of the notification to be dispatched.</typeparam>
  /// <param name="dispatcher">The event dispatcher responsible for sending the notification.</param>
  /// <param name="stream">The event stream ID where the serialized JSON notification will be dispatched.</param>
  /// <param name="notice">The notification object to be serialized and dispatched.</param>
  /// <param name="cancellationToken">An asynchronous operation cancellation token.</param>
  /// <returns>
  ///   A task representing the asynchronous operation, containing a tuple with the original
  ///   <typeparamref name="TNotification" />
  ///   and the serialized JSON string dispatched.
  /// </returns>
  public static Task<(TNotification, string)> DispatchJsonAsync<TNotification>(
    this INoticeIo dispatcher,
    EventStreamId stream,
    TNotification notice,
    CancellationToken cancellationToken = default)
  {
    return dispatcher.DispatchJsonAsync(stream, notice, JsonDefaults.DefaultJsonSerializerOptions, cancellationToken);
  }
}
