using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Custom;

namespace Jds.NiceNotice.Tests.Unit.ExampleApplication;

public class ExampleCustomNoticeValidator(int maxNoticeSize) : NoticeValidator<ExampleCustomBaseEnterpriseEvent>
{
  public override IReadOnlyList<string>? Validate<TEventType>(TEventType notice, string serializedNotice)
  {
    return serializedNotice.Length > maxNoticeSize
      ? new List<string>
      {
        "Notice exceeds maximum allowed size."
      }
      : null;
  }
}
