namespace Jds.NiceNotice.TypedNotices.Metadata;

/// <summary>
///   Implementation of <see cref="NoticeMetadataProvider" />.
/// </summary>
public static class MetadataProviders
{
  /// <summary>
  ///   Gets a default <see cref="NoticeMetadataProvider" />.
  ///   This implementation generates metadata based by invoking <see cref="INoticeMetadata.GetMetadata" />,
  ///   if the notice implements <see cref="INoticeMetadata" />. Otherwise, it returns <c>null</c>.
  /// </summary>
  /// <returns>Returns a default <see cref="NoticeMetadataProvider" />.</returns>
  public static NoticeMetadataProvider DefaultMetadataProvider()
  {
    return new DefaultNoticeMetadataProvider();
  }

  /// <summary>
  ///   Gets a default <see cref="NoticeMetadataProvider{TEnterpriseEventBaseType}" />.
  ///   This implementation generates metadata based by invoking <see cref="INoticeMetadata.GetMetadata" />,
  ///   if the notice implements <see cref="INoticeMetadata" />. Otherwise, it returns <c>null</c>.
  /// </summary>
  /// <returns>Returns a default <see cref="NoticeMetadataProvider{TEnterpriseEventBaseType}" />.</returns>
  public static NoticeMetadataProvider<TEnterpriseEventBaseType> DefaultMetadataProvider<TEnterpriseEventBaseType>()
    where TEnterpriseEventBaseType : notnull
  {
    return new DefaultNoticeMetadataProvider<TEnterpriseEventBaseType>();
  }
}
