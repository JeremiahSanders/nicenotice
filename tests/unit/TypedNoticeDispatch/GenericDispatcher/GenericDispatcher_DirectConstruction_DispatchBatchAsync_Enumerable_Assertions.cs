using System.Net.Mime;
using System.Text.Json;

using Jds.NiceNotice.Dispatching;
using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Custom;
using Jds.NiceNotice.TypedNotices;
using Jds.TestingUtils.Randomization;
using Jds.TestingUtils.Xunit2.Extras;

using Shouldly;

namespace Jds.NiceNotice.Tests.Unit.TypedNoticeDispatch.GenericDispatcher;

public class GenericDispatcher_DirectConstruction_DispatchBatchAsync_Enumerable_Assertions(
  GenericDispatcher_DirectConstruction_DispatchBatchAsync_Enumerable_Assertions.Fixture fixture
)
  : BaseCaseAssertions<GenericDispatcher_DirectConstruction_DispatchBatchAsync_Enumerable_Assertions.Fixture>(fixture)
{
  [Fact]
  public void Act_ResponseIndicatesSuccess()
  {
    //   All the notices should have been returned as a success.
    CaseArrangement.request.ShouldAllBe(kvp =>
      CaseArrangement.result.Successes.Any(response => kvp.Equals(response.TypedNotice))
    );
  }

  [Theory]
  [InlineData(1)]
  [InlineData(2)]
  [InlineData(3)]
  [InlineData(4)]
  public void Verification_IoReceivedExpectedNotices(int caseValue)
  {
    (string valueToFind, object expected) = GetCaseData(caseValue);

    // The notice should have gone to the default stream and it should contain the value to find.
    CaseArrangement.noticeIo.CapturedNotices.ShouldContain(item =>
      item.Stream == (EventStreamId)CaseArrangement.defaultStream &&
      item.Notice.Contains(valueToFind)
    );
  }

  [Theory]
  [InlineData(1)]
  [InlineData(2)]
  [InlineData(3)]
  [InlineData(4)]
  public void Verification_IoReceivedJson(int caseValue)
  {
    (string valueToFind, object expected) = GetCaseData(caseValue);

    // The notice should have been serialized to JSON as expected.
    IoNoticeDispatchRequest actual = CaseArrangement.noticeIo.CapturedNotices.First(item =>
      item.Stream == (EventStreamId)CaseArrangement.defaultStream &&
      item.Notice.Contains(valueToFind)
    );
    string expectedJson = JsonSerializer.Serialize(expected, JsonDefaults.DefaultJsonSerializerOptions);

    actual.Notice.ShouldBe(expectedJson);

    actual.ContentType.ShouldBe(MediaTypeNames.Application.Json);
  }

  [Theory]
  [InlineData(1)]
  [InlineData(2)]
  [InlineData(3)]
  [InlineData(4)]
  public void Verification_IoReceivedMetadata(int caseValue)
  {
    (string valueToFind, object expected) = GetCaseData(caseValue);
    IoNoticeDispatchRequest actual = CaseArrangement.noticeIo.CapturedNotices.First(item =>
      item.Stream == (EventStreamId)CaseArrangement.defaultStream &&
      item.Notice.Contains(valueToFind)
    );
    IReadOnlyDictionary<string, NoticeMetadataValue> meta = actual.Metadata.ShouldNotBeNull();
    meta.Keys.ShouldContain(expected: "name");
  }

  private (string valueToFind, object expected) GetCaseData(int caseValue)
  {
    return caseValue switch
    {
      1 => (CaseArrangement.login1.Username, CaseArrangement.login1),
      2 => (CaseArrangement.login2.Username, CaseArrangement.login2),
      3 => (CaseArrangement.logout3.Username, CaseArrangement.logout3),
      4 => (CaseArrangement.logout4.Username, CaseArrangement.logout4),
      _ => throw new ArgumentOutOfRangeException(nameof(caseValue), caseValue, message: null)
    };
  }

  public class Fixture : GenericDispatcher_DirectConstruction_Fixture
  {
    public readonly ExampleCustomLoginEvent login1;
    public readonly ExampleCustomLoginEvent login2;
    public readonly ExampleCustomLogoutEvent logout3;
    public readonly ExampleCustomLogoutEvent logout4;
    private readonly BatchDispatchOptions options;
    public BatchTypedNoticeDispatchResult result;

    public Fixture()
    {
      // Create some events to dispatch.
      login1 = new ExampleCustomLoginEvent
      {
        Username = "test1"
      };
      login2 = new ExampleCustomLoginEvent
      {
        Username = "test2"
      };
      logout3 = new ExampleCustomLogoutEvent
      {
        Username = "test3"
      };
      logout4 = new ExampleCustomLogoutEvent
      {
        Username = "test4"
      };
      request =
        [login1, login2, logout3, logout4];

      options = new BatchDispatchOptions
      {
        MaxDegreeOfParallelism = Randomizer.Shared.IntInRange(minInclusive: 1, maxExclusive: 9)
      };
      result = null!;
    }

    public List<ExampleCustomBaseEnterpriseEvent> request { get; set; }

    protected override async Task ActAsync()
    {
      // Act
      result =
        await ee.DispatchBatchAsync(request, options);
      result = await ee.DispatchBatchAsync(
        request
      );
    }
  }
}
