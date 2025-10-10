namespace Jds.NiceNotice;

/// <summary>
///   An abstraction representing the algorithm used for selecting a logical event stream
///   (identified by <see cref="EventStreamId" />) for a notice,
///   a process sometimes referred to as routing.
/// </summary>
/// <typeparam name="TEnterpriseEventBaseType"></typeparam>
public abstract class NoticeStreamSelector<TEnterpriseEventBaseType> where TEnterpriseEventBaseType : notnull
{
  /// <summary>
  ///   Determines the logical event stream for the given notice.
  ///   (This process is sometimes referred to as routing.)
  /// </summary>
  /// <remarks>
  ///   Note that <see cref="EventStreamId" /> is a logical token; it is not a direct reference to a physical stream.
  ///   Notice dispatch I/O implementations determine how the logical stream is mapped to a
  ///   physical stream (e.g., an SNS topic).
  /// </remarks>
  /// <param name="notice">The notice to be routed.</param>
  /// <typeparam name="TEventType">
  ///   The notice object type, a subtype of <typeparamref name="TEnterpriseEventBaseType" />.
  /// </typeparam>
  /// <returns>Returns the logical stream identifier.</returns>
  public abstract EventStreamId GetStreamId<TEventType>(TEventType notice) where TEventType : TEnterpriseEventBaseType;
}
