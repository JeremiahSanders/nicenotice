using System.Text.Json;
using System.Text.Json.Serialization;

using Jds.TestingUtils.Randomization;

using Shouldly;

using Xunit.Abstractions;

namespace Jds.NiceNotice.Tests.Unit;

public class EnterpriseEventTests(ITestOutputHelper outputHelper)
{
  [Fact]
  public void CanOverrideEventSchemaDuringConstruction()
  {
    JsonSerializerOptions options = JsonDefaults.DefaultJsonSerializerOptions;

    const string customEventSchema = "custom-event-schema";
    GoalScoredEvent customEvent = GenerateGoalScoredEvent() with
    {
      Schema = customEventSchema
    };

    // Act
    string serialized = JsonSerializer.Serialize(customEvent, options);
    outputHelper.WriteLine($"Serialized:{Environment.NewLine}{serialized}");
    GoalScoredEvent? deserialized = JsonSerializer.Deserialize<GoalScoredEvent>(serialized, options);

    // Assert
    serialized.ShouldNotBeNullOrWhiteSpace();
    deserialized
      .ShouldNotBeNull()
      .ShouldBeEquivalentTo(customEvent);
    deserialized.Schema.ShouldBe(customEventSchema);
  }

  [Fact]
  public void CanSerializeCustomEnterpriseEvents()
  {
    JsonSerializerOptions options = JsonDefaults.DefaultJsonSerializerOptions;

    GoalScoredEvent customEvent = GenerateGoalScoredEvent();

    // Act
    string serialized = JsonSerializer.Serialize(customEvent, options);
    outputHelper.WriteLine($"Serialized:{Environment.NewLine}{serialized}");
    GoalScoredEvent? deserialized = JsonSerializer.Deserialize<GoalScoredEvent>(serialized, options);

    // Assert
    serialized.ShouldNotBeNullOrWhiteSpace();
    deserialized
      .ShouldNotBeNull()
      .ShouldBeEquivalentTo(customEvent);
    deserialized.Schema.ShouldBe(expected: "GoalScored@3");
  }

  [Fact]
  public void SerializedEventsDoNotEscapeCommonCharacters()
  {
    JsonSerializerOptions options = JsonDefaults.DefaultJsonSerializerOptions;

    const string commonCharacters = "+++";
    GoalScoredEvent customEvent = GenerateGoalScoredEvent() with
    {
      PlayerName = commonCharacters
    };

    // Act
    string serialized = JsonSerializer.Serialize(customEvent, options);
    outputHelper.WriteLine($"Serialized:{Environment.NewLine}{serialized}");
    GoalScoredEvent? deserialized = JsonSerializer.Deserialize<GoalScoredEvent>(serialized, options);

    // Assert
    serialized.ShouldNotBeNullOrWhiteSpace();
    deserialized
      .ShouldNotBeNull()
      .ShouldBeEquivalentTo(customEvent);
    deserialized.PlayerName.ShouldBe(
      commonCharacters,
      customMessage: "Because the serializer decodes escaped characters"
    );
    serialized.ShouldContain(commonCharacters, customMessage: "because we are using relaxed encoding");
  }

  private static GoalScoredEvent GenerateGoalScoredEvent()
  {
    return new GoalScoredEvent
    {
      PlayerName = Randomizer.Shared.DemographicsForenameUsa(),
      TeamName = Randomizer.Shared.RandomStringLatin(length: 6),
      Points = Randomizer.Shared.IntInRange(minInclusive: 1, maxExclusive: 4)
    };
  }

  /// <summary>
  ///   An example enterprise event.
  /// </summary>
  public record GoalScoredEvent : GameEvent
  {
    /// <summary>
    ///   Gets the team's new cumulative point total (i.e., including this goal scored event).
    /// </summary>
    public int? NewScore { get; init; }

    /// <summary>
    ///   Gets the name of the player who scored the goal.
    /// </summary>
    public string PlayerName { get; init; } = string.Empty;

    /// <summary>
    ///   Gets the point value of the goal which was scored.
    /// </summary>
    public int Points { get; init; }

    /// <summary>
    ///   Gets the team's prior cumulative point total.
    /// </summary>
    public int? PriorScore { get; init; }

    /// <summary>
    ///   Gets the name of the team which scored the goal.
    /// </summary>
    public string TeamName { get; init; } = string.Empty;

    protected override int? SchemaRevision => 3;
    protected override string SchemaTitle => "GoalScored";
  }

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
    ///   Gets the name of the away team in the game.
    /// </summary>
    [JsonPropertyName(name: "awayTeam")]
    public string? AwayTeam { get; init; }

    /// <summary>
    ///   Gets the name of the home team in the game.
    /// </summary>
    [JsonPropertyName(name: "homeTeam")]
    public string? HomeTeam { get; init; }
  }
}
