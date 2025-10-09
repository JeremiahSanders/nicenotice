namespace Jds.NiceNotice;

public abstract class NoticeValidator<TEnterpriseEventBaseType>
  where TEnterpriseEventBaseType : notnull
{
  public abstract IReadOnlyList<string>? Validate<TEventType>(TEventType notice, string serializedNotice)
    where TEventType : TEnterpriseEventBaseType;
}

public abstract class NoticeValidator
{
  public abstract IReadOnlyList<string>? Validate<TEventType>(TEventType notice, string serializedNotice)
    where TEventType : notnull;
}
