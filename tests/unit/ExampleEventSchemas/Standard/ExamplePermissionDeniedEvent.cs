using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Standard;

/// <summary>
///   An example event representing a situation where the user attempted an action,
///   but lacked one or more required permissions.
/// </summary>
public record ExamplePermissionDeniedEvent : EnterpriseEvent
{
  [Required]
  [MinLength(length: 1)]
  [JsonPropertyName(name: "missingPermissions")]
  public string[] MissingPermissions { get; init; } = [];

  [Required]
  [MinLength(length: 1)]
  [JsonPropertyName(name: "requiredPermissions")]
  public string[] RequiredPermissions { get; init; } = [];

  [Required(AllowEmptyStrings = false)]
  [JsonPropertyName(name: "username")]
  public required string Username { get; init; } = string.Empty;
}
