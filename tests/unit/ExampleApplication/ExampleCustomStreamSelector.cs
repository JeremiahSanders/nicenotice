using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Custom;

namespace Jds.NiceNotice.Tests.Unit.ExampleApplication;

public class ExampleCustomStreamSelector : NoticeStreamSelector<ExampleCustomBaseEnterpriseEvent>
{
  public EventStreamId UserSessionStream { get; init; } = EventStreamId.From(value: "user-session");
  public EventStreamId DefaultStream { get; init; } = EventStreamId.From(value: "default");

  public override EventStreamId GetStreamId<TEventType>(TEventType notice)
  {
    return notice switch
    {
      ExampleCustomLoginEvent login => UserSessionStream,
      ExampleCustomLogoutEvent logout => UserSessionStream,
      _ => DefaultStream
    };
  }
}
