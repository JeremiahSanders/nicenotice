using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Custom;

public record ExampleCustomLogoutEvent : ExampleCustomBaseEnterpriseEvent
{
  public ExampleCustomLogoutEvent()
  {
    Name = "Logout";
  }

  [JsonPropertyName(name: "username")]
  [Required(AllowEmptyStrings = false)]
  public required string Username { get; init; }
}
