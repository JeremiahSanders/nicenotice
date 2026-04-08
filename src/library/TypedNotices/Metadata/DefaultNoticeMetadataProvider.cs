namespace Jds.NiceNotice.TypedNotices.Metadata;

/// <summary>
///   A default implementation of <see cref="NoticeMetadataProvider{TEnterpriseEventBaseType}" />.
///   This implementation generates metadata based by invoking <see cref="INoticeMetadata.GetMetadata" />,
///   if the notice implements <see cref="INoticeMetadata" />. Otherwise, it returns <c>null</c>.
/// </summary>
/// <typeparam name="TEnterpriseEventBaseType">The base event type.</typeparam>
internal class
  DefaultNoticeMetadataProvider<TEnterpriseEventBaseType> : NoticeMetadataProvider<TEnterpriseEventBaseType>
  where TEnterpriseEventBaseType : notnull
{
  public override IReadOnlyDictionary<string, NoticeMetadataValue>? GetMetadata<TEventType>(
    TEventType notice,
    string serializedNotice,
    string? serializedContentType
  )
  {
    if (notice is INoticeMetadata metadata)
    {
      return metadata.GetMetadata();
    }

    return null;
  }
}

/// <summary>
///   A default implementation of <see cref="NoticeMetadataProvider" />.
///   This implementation generates metadata based by invoking <see cref="INoticeMetadata.GetMetadata" />,
///   if the notice implements <see cref="INoticeMetadata" />. Otherwise, it returns <c>null</c>.
/// </summary>
internal class DefaultNoticeMetadataProvider : NoticeMetadataProvider
{
  public override IReadOnlyDictionary<string, NoticeMetadataValue>? GetMetadata<TEventType>(
    TEventType notice,
    string serializedNotice,
    string? serializedContentType
  )
  {
    if (notice is INoticeMetadata metadata)
    {
      return metadata.GetMetadata();
    }

    return null;
  }
}
