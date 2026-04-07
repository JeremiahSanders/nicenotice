using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Standard;

public record ExampleLoginEnterpriseEvent : EnterpriseEvent
{
  [Required(AllowEmptyStrings = false)]
  [JsonPropertyName(name: "username")]
  public required string Username { get; init; } = string.Empty;

  protected override string SchemaTitle => "Login";

  public override IReadOnlyDictionary<string, NoticeMetadataValue>? GetMetadata()
  {
    return new Dictionary<string, NoticeMetadataValue>
    {
      {
        "schema", SchemaTitle
      }
    };
  }
}
