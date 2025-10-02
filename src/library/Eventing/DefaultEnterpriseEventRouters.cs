namespace Jds.NiceNotice;

public static class DefaultEnterpriseEventRouters
{
  public static Func<TEnterpriseEventBaseType, EventStreamId> FromMap<TEnterpriseEventBaseType>(
    IReadOnlyDictionary<Type, EventStreamId>? map = null,
    EventStreamId? defaultStream = null)
    where TEnterpriseEventBaseType : notnull
  {
    return notice =>
    {
      Type noticeType = notice.GetType();
      try
      {
        if (map == null)
        {
          throw new StreamDeterminationException(
            $"No stream map defined. Cannot determine stream for event type {noticeType.Name}.",
            innerException: null
          );
        }

        return map[noticeType];
      }
      catch (Exception e)
      {
        if (defaultStream != null)
        {
          return defaultStream.Value;
        }

        throw new StreamDeterminationException($"No stream defined for event type {noticeType.Name}.", e);
      }
    };
  }
}