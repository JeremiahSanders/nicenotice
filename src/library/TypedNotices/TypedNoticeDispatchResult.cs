namespace Jds.NiceNotice.TypedNotices;

/// <summary>
///   The result of dispatching a typed notice.
/// </summary>
/// <typeparam name="TEventType">The enterprise event notice type.</typeparam>
public record TypedNoticeDispatchResult<TEventType> where TEventType : notnull
{
  /// <summary>
  ///   Gets any exception that occurred during the dispatch.
  /// </summary>
  public Exception? Exception { get; init; }

  /// <summary>
  ///   Gets the I/O notice request which was sent to the I/O dispatcher.
  /// </summary>
  public required IoNoticeDispatchRequest IoRequest { get; init; }

  /// <summary>
  ///   Gets a value indicating whether the dispatch was successful, based on the absence of an exception.
  /// </summary>
  public bool IsSuccessful => Exception == null;

  /// <summary>
  ///   Gets the typed notice.
  /// </summary>
  public required TEventType Notice { get; init; }

  /// <summary>
  ///   Requires that the dispatch was successful
  ///   (as indicated by <see cref="IsSuccessful" /> and absence of an <see cref="Exception" />),
  ///   throwing <see cref="Exception" /> if not.
  /// </summary>
  /// <returns>Returns this instance.</returns>
  /// <exception cref="Exception">Thrown if <see cref="Exception" /> is not <c>null</c>.</exception>
  public TypedNoticeDispatchResult<TEventType> RequireSuccess()
  {
    return Exception != null ? throw Exception : this;
  }
}
