namespace Jds.NiceNotice;

internal class ConstantStreamSelector<TEnterpriseEventBaseType>(EventStreamId stream)
  : NoticeStreamSelector<TEnterpriseEventBaseType> where TEnterpriseEventBaseType : notnull
{
  public override EventStreamId GetStreamId<TEventType>(TEventType notice)
  {
    return stream;
  }
}
