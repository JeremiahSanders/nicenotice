namespace Jds.NiceNotice;

internal class DefaultTypedNoticeDispatcher(
  INoticeIo ioDispatcher,
  NoticeSerializer noticeSerializer
) : TypedNoticeDispatcher(ioDispatcher)
{
  protected override string SerializeNotice<TEventType>(TEventType notice)
  {
    return noticeSerializer.Serialize(notice);
  }
}

internal class DefaultTypedNoticeDispatcher<TEnterpriseEventBaseType>(
  INoticeIo ioDispatcher,
  NoticeStreamSelector<TEnterpriseEventBaseType> streamSelector,
  NoticeSerializer<TEnterpriseEventBaseType> notificationSerializer,
  NoticeValidator<TEnterpriseEventBaseType> validateNotice
)
  : TypedNoticeDispatcher<TEnterpriseEventBaseType>(ioDispatcher)
  where TEnterpriseEventBaseType : notnull
{
  protected override EventStreamId GetStreamId(TEnterpriseEventBaseType notice)
  {
    return streamSelector.GetStreamId(notice);
  }

  protected override string SerializeNotice<TEventType>(TEventType notice)
  {
    return notificationSerializer.Serialize(notice);
  }

  protected override IReadOnlyList<string>? ValidateNotice<TEventType>(TEventType notice, string serializedNotice)
  {
    return validateNotice.Validate(notice, serializedNotice);
  }
}
