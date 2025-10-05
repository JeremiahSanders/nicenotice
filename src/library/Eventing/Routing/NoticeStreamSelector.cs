namespace Jds.NiceNotice;

public abstract class NoticeStreamSelector<TEnterpriseEventBaseType> where TEnterpriseEventBaseType : notnull
{
  public abstract EventStreamId GetStreamId<TEventType>(TEventType notice) where TEventType : TEnterpriseEventBaseType;
}
