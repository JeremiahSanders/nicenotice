using System.Text.Json.Serialization;

namespace Jds.NiceNotice.Tests.Unit.ExampleEventSchemas;

/// <summary>
///   An example base event for a used to support a team sport.
/// </summary>
public record GameEvent : EnterpriseEvent
{
  // ReSharper disable once MemberCanBeProtected.Global
  public GameEvent()
  {
    base.SchemaRevision = 0;
  }

  /// <summary>
  ///   Gets the name of the home team in the game.
  /// </summary>
  [JsonPropertyName(name: "homeTeam")]
  public string? HomeTeam { get; init; }

  /// <summary>
  ///   Gets the name of the away team in the game.
  /// </summary>
  [JsonPropertyName(name: "awayTeam")]
  public string? AwayTeam { get; init; }
}
