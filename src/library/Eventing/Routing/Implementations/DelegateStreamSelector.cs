namespace Jds.NiceNotice;

internal class DelegateStreamSelector<TEnterpriseEventBaseType>(Func<TEnterpriseEventBaseType, EventStreamId> selector)
  : NoticeStreamSelector<TEnterpriseEventBaseType> where TEnterpriseEventBaseType : notnull
{
  public override EventStreamId GetStreamId<TEventType>(TEventType notice)
  {
    return selector(notice);
  }
}
