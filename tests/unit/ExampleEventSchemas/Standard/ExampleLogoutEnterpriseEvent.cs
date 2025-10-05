using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Standard;

public record ExampleLogoutEnterpriseEvent : EnterpriseEvent
{
  protected override string SchemaTitle => "Logout";
  protected override int? SchemaRevision { get; init; } = 1;

  [Required(AllowEmptyStrings = false)]
  [JsonPropertyName(name: "username")]
  public required string Username { get; init; } = string.Empty;

  /// <remarks>Added in <see cref="SchemaRevision" /> <c>1</c>.</remarks>
  [JsonPropertyName(name: "duration")]
  public TimeSpan? SessionDuration { get; init; }
}
