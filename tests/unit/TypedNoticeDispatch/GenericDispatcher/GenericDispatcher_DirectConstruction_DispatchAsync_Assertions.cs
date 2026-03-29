using System.Net.Mime;
using System.Text.Json;

using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Custom;
using Jds.NiceNotice.TypedNotices;
using Jds.TestingUtils.Xunit2.Extras;

using Shouldly;

namespace Jds.NiceNotice.Tests.Unit.TypedNoticeDispatch.GenericDispatcher;

public class GenericDispatcher_DirectConstruction_DispatchAsync_Assertions
  : BaseCaseAssertions<GenericDispatcher_DirectConstruction_DispatchAsync_Assertions.Fixture>
{
  public GenericDispatcher_DirectConstruction_DispatchAsync_Assertions(Fixture caseArrangementFixture)
    : base(caseArrangementFixture)
  {
  }

  [Fact]
  public void Act_ReturnsExpectedNotice()
  {
    CaseArrangement.result.Notice.ShouldBeEquivalentTo(CaseArrangement.toDispatch);
  }

  [Fact]
  public void Verification_IoReceivedExpectedNotice()
  {
    CaseArrangement.noticeIo.CapturedNotices.ShouldContain(item =>
      item.Stream == (EventStreamId)CaseArrangement.defaultStream &&
      item.Notice.Contains(CaseArrangement.toDispatch.Username)
    );
  }

  [Fact]
  public void Verification_IoReceivedJson()
  {
    IoRequestNotice captured = CaseArrangement.noticeIo.CapturedNotices.Single(item =>
      item.Stream == (EventStreamId)CaseArrangement.defaultStream &&
      item.Notice.Contains(CaseArrangement.toDispatch.Username)
    );
    captured.ContentType.ShouldBe(MediaTypeNames.Application.Json);
    ExampleCustomLoginEvent? fromSerialized = JsonSerializer.Deserialize<ExampleCustomLoginEvent>(
      captured.Notice,
      JsonDefaults.DefaultJsonSerializerOptions
    );
    fromSerialized.ShouldBeEquivalentTo(CaseArrangement.toDispatch);
  }

  [Fact]
  public void Verification_IoReceivedMetadata()
  {
    CaseArrangement.noticeIo.CapturedNotices.ShouldAllBe(capturedNotice =>
      capturedNotice.Metadata != null &&
      capturedNotice.Metadata.ContainsKey("name") &&
      capturedNotice.Metadata.ContainsKey("ts")
    );
  }

  public class Fixture : GenericDispatcher_DirectConstruction_Fixture
  {
    public readonly ExampleCustomLoginEvent toDispatch;
    public TypedNoticeDispatchResult<ExampleCustomLoginEvent> result;

    public Fixture()
    {
      toDispatch = new ExampleCustomLoginEvent
      {
        Username = "test"
      };
      result = null!;
    }

    protected override async Task ActAsync()
    {
      await base.ActAsync();

      // Act
      result = await ee.DispatchAsync(toDispatch);
    }
  }
}
