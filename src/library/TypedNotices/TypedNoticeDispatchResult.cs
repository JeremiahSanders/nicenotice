namespace Jds.NiceNotice.TypedNotices;

/// <summary>
///   The result of dispatching a typed notice.
/// </summary>
/// <typeparam name="TEventType">The enterprise event notice type.</typeparam>
public record TypedNoticeDispatchResult<TEventType> where TEventType : notnull
{
  /// <summary>
  ///   Gets the response from the I/O dispatcher.
  /// </summary>
  /// <remarks>This value was returned from <see cref="INoticeIo.DispatchAsync" />.</remarks>
  public required string IoResponse { get; init; }

  /// <summary>
  ///   Gets the typed notice.
  /// </summary>
  public required TEventType Notice { get; init; }

  /// <summary>
  ///   Gets the serialized notice.
  /// </summary>
  /// <remarks>This value was sent to I/O <see cref="INoticeIo.DispatchAsync" />.</remarks>
  public required string Serialized { get; init; }

  /// <summary>
  ///   Gets the stream to which the notice was dispatched.
  /// </summary>
  public required EventStreamId Stream { get; init; }
}
