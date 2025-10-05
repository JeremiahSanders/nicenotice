using System.Text.Json.Serialization;

namespace Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Custom;

public record ExampleCustomLoginEvent : ExampleCustomBaseEnterpriseEvent
{
  public ExampleCustomLoginEvent()
  {
    Name = CreateEventName(eventTitle: "Login", eventSchemaRevision: 0);
  }

  [JsonPropertyName(name: "username")]
  public required string Username { get; init; }
}
