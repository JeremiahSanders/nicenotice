namespace Jds.NiceNotice.TypedNotices;

/// <summary>
///   An attribute indicating the logical identifier of the notice stream to which a notice type should be sent.
///   I.e., the <see cref="EventStreamId" /> value.
/// </summary>
/// <remarks>
///   <para>
///     This attribute is intended to be used on notice types.
///     I.e., objects which descend from <see cref="EnterpriseEvent" /> or the runtime application's equivalent.
///   </para>
/// </remarks>
/// <param name="streamName">
///   The logical identifier of the notice stream to which this notice type should be sent.
///   I.e., the <see cref="EventStreamId" /> value.
///   (E.g., <c>user-session</c>)
///   NOT an I/O address.
/// </param>
[AttributeUsage(AttributeTargets.Class)]
public class NoticeStreamAttribute(string streamName) : Attribute
{
  /// <summary>
  ///   Gets the logical identifier of the notice stream to which this notice type should be sent.
  ///   I.e., the <see cref="EventStreamId" /> value.
  ///   (E.g., <c>user-session</c>)
  /// </summary>
  /// <remarks>NOT an I/O address.</remarks>
  public string? StreamName { get; } = string.IsNullOrWhiteSpace(streamName) ? null : streamName.Trim();
}
