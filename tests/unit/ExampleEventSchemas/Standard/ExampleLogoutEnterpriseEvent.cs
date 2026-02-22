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

  protected override int? SchemaRevision { get; init; } = 1;
  protected override string SchemaTitle => "Logout";
}
