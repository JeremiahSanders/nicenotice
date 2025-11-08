namespace Jds.NiceNotice.Dispatching;

/// <summary>
///   A notice that is part of a batch I/O request.
/// </summary>
/// <param name="stream">A logical event stream to which the notice is routed.</param>
/// <param name="notice">The content of the notice which is being dispatched.</param>
public class BatchedIoRequestNotice(EventStreamId stream, string notice)
{
  /// <summary>
  ///   Gets the logical event stream to which the notice is routed.
  /// </summary>
  public EventStreamId Stream { get; } = stream;

  /// <summary>
  ///   Gets the content of the notice which is being dispatched.
  /// </summary>
  public string Notice { get; } = notice;
}
