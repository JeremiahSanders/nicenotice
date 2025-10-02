namespace Jds.NiceNotice.Tests.Unit.ExampleApplication;

public record ExampleLoginEvent : ExampleBaseEnterpriseEvent
{
  public ExampleLoginEvent()
  {
    Name = CreateEventName(eventTitle: "Login", eventSchemaRevision: 0);
  }

  public required string Username { get; init; }
}
