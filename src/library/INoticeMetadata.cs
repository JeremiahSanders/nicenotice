namespace Jds.NiceNotice;

/// <summary>
///   Interface which can be applied to typed notices to generate custom metadata
///   which can be included when being dispatched to I/O.
/// </summary>
public interface INoticeMetadata
{
  /// <summary>
  ///   Gets metadata for this notice.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     This method supports generating values which will be passed to the configured notice I/O
  ///     via <see cref="IoNoticeDispatchRequest.Metadata" />.
  ///   </para>
  /// </remarks>
  /// <returns>Gets the metadata for this notice.</returns>
  IReadOnlyDictionary<string, NoticeMetadataValue>? GetMetadata();
}
