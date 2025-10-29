namespace Jds.NiceNotice;

/// <summary>
///   A routed notice that is part of a batch.
/// </summary>
/// <param name="stream">A logical event stream identifier.</param>
/// <param name="notice">A notice object.</param>
public class BatchedRoutedTypedNotice(EventStreamId stream, object notice)
{
  /// <summary>
  ///   Gets the logical event stream to which the notice is routed.
  /// </summary>
  public EventStreamId Stream { get; } = stream;

  /// <summary>
  ///   Gets the notice which is being dispatched.
  /// </summary>
  public object Notice { get; } = notice;
}
