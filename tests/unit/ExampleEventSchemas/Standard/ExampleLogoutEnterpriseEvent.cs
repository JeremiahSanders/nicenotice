using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Standard;

public record ExampleLogoutEnterpriseEvent : EnterpriseEvent
{
  /// <remarks>Added in <see cref="SchemaRevision" /> <c>1</c>.</remarks>
  [JsonPropertyName(name: "duration")]
  public TimeSpan? SessionDuration { get; init; }

  [Required(AllowEmptyStrings = false)]
  [JsonPropertyName(name: "username")]
  public required string Username { get; init; } = string.Empty;

  public override IReadOnlyDictionary<string, NoticeMetadataValue>? GetMetadata()
  {
    return new Dictionary<string, NoticeMetadataValue>
    {
      {
        "duration", SessionDuration?.ToString() ?? string.Empty
      }
    };
  }
}
