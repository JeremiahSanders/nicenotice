using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Custom;
using Jds.NiceNotice.TypedNotices.Serialization;

namespace Jds.NiceNotice.Tests.Unit.ExampleApplication;

public class ExampleCustomNoticeSerializer : NoticeSerializer<ExampleCustomBaseEnterpriseEvent>
{
  public override string ContentType => "text/plain";

  public override string Serialize<TEventType>(TEventType notice)
  {
    // Since the base event is a record, we can get human-readable output -- as long as our model is limited to primitives or other records of primitives.
    // It's not JSON, but it's readable.
    return notice.ToString();
  }
}
