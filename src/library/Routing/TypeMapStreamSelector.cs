namespace Jds.NiceNotice;

internal class TypeMapStreamSelector<TEnterpriseEventBaseType>(
  IReadOnlyDictionary<Type, EventStreamId> map,
  EventStreamId? defaultStream = null
)
  : NoticeStreamSelector<TEnterpriseEventBaseType> where TEnterpriseEventBaseType : notnull
{
  public override EventStreamId GetStreamId<TEventType>(TEventType notice)
  {
    Type noticeType = notice.GetType();

    if (!map.TryGetValue(noticeType, out EventStreamId id))
    {
      if (defaultStream != null)
      {
        return defaultStream.Value;
      }

      throw new StreamDeterminationException(
        $"No stream defined for event type {noticeType.Name}.",
        innerException: null
      );
    }

    return id;
  }
}
