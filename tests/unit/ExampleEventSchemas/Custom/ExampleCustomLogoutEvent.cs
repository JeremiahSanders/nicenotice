using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Custom;

public record ExampleCustomLogoutEvent : ExampleCustomBaseEnterpriseEvent
{
  public ExampleCustomLogoutEvent()
  {
    Name = CreateEventName(eventTitle: "Logout", eventSchemaRevision: 0);
  }

  [JsonPropertyName(name: "username")]
  [Required(AllowEmptyStrings = false)]
  public required string Username { get; init; }
}
