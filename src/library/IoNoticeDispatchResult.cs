namespace Jds.NiceNotice;

/// <summary>
///   A result of dispatching a notice to I/O.
/// </summary>
/// <param name="stream">The stream to which the notice was dispatched.</param>
/// <param name="notice">The notice content.</param>
/// <param name="metadata">Optional metadata associated with the notice.</param>
/// <param name="contentType">The content type of the notice, e.g., <c>text/plain</c>.</param>
/// <param name="exception">
///   Any exception that occurred during the dispatch.
///   If null, the dispatch is considered successful.
/// </param>
public class IoNoticeDispatchResult(
  EventStreamId stream,
  string notice,
  IReadOnlyDictionary<string, string>? metadata,
  string? contentType,
  Exception? exception
)
  : IoRequestNotice(stream, notice, metadata, contentType)
{
  /// <summary>
  ///   Gets any exception that occurred during the dispatch.
  /// </summary>
  public Exception? Exception { get; } = exception;

  /// <summary>
  ///   Gets a value indicating whether the dispatch was successful, based on the absence of an exception.
  /// </summary>
  public bool IsSuccessful => Exception == null;

  /// <inheritdoc />
  public override bool Equals(object? obj)
  {
    if (obj is null)
    {
      return false;
    }

    if (ReferenceEquals(this, obj))
    {
      return true;
    }

    if (obj.GetType() != GetType())
    {
      return false;
    }

    return Equals((IoNoticeDispatchResult)obj);
  }

  /// <inheritdoc />
  public override int GetHashCode()
  {
    return HashCode.Combine(base.GetHashCode(), Exception);
  }

  /// <summary>
  ///   Requires that the dispatch was successful
  ///   (as indicated by <see cref="IsSuccessful" /> and absence of an <see cref="Exception" />),
  ///   throwing <see cref="Exception" /> if not.
  /// </summary>
  /// <returns>Returns this instance.</returns>
  /// <exception cref="Exception">Thrown if <see cref="Exception" /> is not <c>null</c>.</exception>
  public IoNoticeDispatchResult RequireSuccess()
  {
    return Exception != null ? throw Exception : this;
  }

  /// <summary>
  ///   Determines whether the specified object is equal to the current object.
  /// </summary>
  /// <param name="other">The other object to compare against this instance.</param>
  /// <returns>Returns true if considered equal, or false otherwise.</returns>
  protected bool Equals(IoNoticeDispatchResult other)
  {
    return base.Equals(other) && Equals(Exception, other.Exception);
  }
}
