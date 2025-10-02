namespace Jds.NiceNotice;

public abstract class NoticeSerializer
{
  public abstract string Serialize<TEventType>(TEventType notice) where TEventType : notnull;
}

public abstract class NoticeSerializer<TEnterpriseEventBaseType>
{
  public abstract string Serialize<TEventType>(TEventType notice)
    where TEventType : notnull, TEnterpriseEventBaseType;
}
