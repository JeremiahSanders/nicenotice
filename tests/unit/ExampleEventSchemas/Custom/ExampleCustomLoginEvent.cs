using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Custom;

public record ExampleCustomLoginEvent : ExampleCustomBaseEnterpriseEvent
{
  public ExampleCustomLoginEvent()
  {
    Name = "Login";
  }

  [JsonPropertyName(name: "username")]
  [Required(AllowEmptyStrings = false)]
  public required string Username { get; init; }
}
