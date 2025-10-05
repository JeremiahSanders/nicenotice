namespace Jds.NiceNotice.Tests.Unit.ExampleEventSchemas;

/// <summary>
///   An example enterprise event.
/// </summary>
public record GoalScoredEvent : GameEvent
{
  protected override int? SchemaRevision => 3;
  protected override string SchemaTitle => "GoalScored";

  /// <summary>
  ///   Gets the point value of the goal which was scored.
  /// </summary>
  public int Points { get; init; }

  /// <summary>
  ///   Gets the name of the player who scored the goal.
  /// </summary>
  public string PlayerName { get; init; } = string.Empty;

  /// <summary>
  ///   Gets the team's prior cumulative point total.
  /// </summary>
  public int? PriorScore { get; init; }

  /// <summary>
  ///   Gets the team's new cumulative point total (i.e., including this goal scored event).
  /// </summary>
  public int? NewScore { get; init; }

  /// <summary>
  ///   Gets the name of the team which scored the goal.
  /// </summary>
  public string TeamName { get; init; } = string.Empty;
}
