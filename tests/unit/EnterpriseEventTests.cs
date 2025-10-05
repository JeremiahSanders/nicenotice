using System.Text.Json;

using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas;
using Jds.TestingUtils.Randomization;

using Shouldly;

using Xunit.Abstractions;

namespace Jds.NiceNotice.Tests.Unit;

public class EnterpriseEventTests(ITestOutputHelper outputHelper)
{
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

  private static GoalScoredEvent GenerateGoalScoredEvent()
  {
    return new GoalScoredEvent
    {
      PlayerName = Randomizer.Shared.DemographicsForenameUsa(),
      TeamName = Randomizer.Shared.RandomStringLatin(length: 6),
      Points = Randomizer.Shared.IntInRange(minInclusive: 1, maxExclusive: 4)
    };
  }
}
