using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Standard;

public record ExampleLoginEnterpriseEvent : EnterpriseEvent
{
  protected override string SchemaTitle => "Login";

  [Required]
  [JsonPropertyName(name: "username")]
  public required string Username { get; init; } = string.Empty;
}
