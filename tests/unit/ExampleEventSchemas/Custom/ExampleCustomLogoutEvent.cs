using System.Text.Json.Serialization;

namespace Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Custom;

public record ExampleCustomLogoutEvent : ExampleCustomBaseEnterpriseEvent
{
  public ExampleCustomLogoutEvent()
  {
    Name = CreateEventName(eventTitle: "Logout", eventSchemaRevision: 0);
  }

  [JsonPropertyName(name: "username")]
  public required string Username { get; init; }
}
