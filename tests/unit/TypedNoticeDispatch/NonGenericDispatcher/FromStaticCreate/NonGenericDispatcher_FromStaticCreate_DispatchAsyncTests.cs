using System.Net.Mime;
using System.Text.Json;

using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Custom;
using Jds.NiceNotice.TypedNotices;
using Jds.TestingUtils.Xunit2.Extras;

using Shouldly;

using Xunit.Abstractions;

namespace Jds.NiceNotice.Tests.Unit.TypedNoticeDispatch.NonGenericDispatcher.FromStaticCreate;

public class NonGenericDispatcher_FromStaticCreate_DispatchAsyncTests
  : BaseCaseAssertions<NonGenericDispatcher_FromStaticCreate_DispatchAsyncTests.Fixture>
{
  public NonGenericDispatcher_FromStaticCreate_DispatchAsyncTests(
    Fixture caseArrangementFixture,
    ITestOutputHelper testOutputHelper)
    : base(caseArrangementFixture)
  {
    testOutputHelper.OutputNotices(caseArrangementFixture.ArrangedNoticeIo.CapturedNotices);
  }

  [Fact]
  public void Act_ReturnsExpectedNotice()
  {
    CaseArrangement.ActResponse.Notice.ShouldBeEquivalentTo(CaseArrangement.ArrangedEvent);
  }

  [Fact]
  public void IoRequestHasMetadata()
  {
    CaseArrangement.ActResponse.IoRequest.Metadata.ShouldNotBeNull();
    CaseArrangement.ActResponse.IoRequest.Metadata.ContainsKey(key: "name").ShouldBeTrue();
    CaseArrangement.ActResponse.IoRequest.Metadata.ContainsKey(key: "ts").ShouldBeTrue();
  }

  [Fact]
  public void IoRequestSerializesToJsonByDefault()
  {
    CaseArrangement.ActResponse.IoRequest.Notice.ShouldNotBeNullOrWhiteSpace();
    CaseArrangement.ActResponse.IoRequest.ContentType.ShouldBe(MediaTypeNames.Application.Json);
    ExampleCustomLoginEvent parsedFromJson = CaseArrangement.ActResponse.DeserializeIoResponseAsJson();
    parsedFromJson.ShouldBeEquivalentTo(CaseArrangement.ArrangedEvent);
  }

  [Fact]
  public void IoRequestStreamMatchesDirectTypeName()
  {
    CaseArrangement.ActResponse.IoRequest.Stream.ShouldBe(CaseArrangement.ArrangedDefaultStreamId);
  }

  [Fact]
  public void Verification_IoReceivedExpectedContentType()
  {
    CaseArrangement.ArrangedNoticeIo.CapturedNotices.ShouldAllBe(capturedNotice =>
      capturedNotice.ContentType == MediaTypeNames.Application.Json
    );
  }

  [Fact]
  public void Verification_IoReceivedExpectedMetadata()
  {
    CaseArrangement.ArrangedNoticeIo.CapturedNotices.ShouldAllBe(capturedNotice =>
      capturedNotice.Metadata != null
      && capturedNotice.Metadata.ContainsKey("name")
      && capturedNotice.Metadata.ContainsKey("ts")
    );
  }

  [Fact]
  public void Verification_IoReceivedNotice()
  {
    CaseArrangement.ArrangedNoticeIo.CapturedNotices.ShouldContain(item =>
      item.Stream == CaseArrangement.ArrangedDefaultStreamId &&
      item.Notice.Contains(CaseArrangement.ArrangedEvent.Username)
    );
    IoRequestNotice captured = CaseArrangement.ArrangedNoticeIo.CapturedNotices.Single(item =>
      item.Stream == CaseArrangement.ArrangedDefaultStreamId &&
      item.Notice.Contains(CaseArrangement.ArrangedEvent.Username)
    );
    ExampleCustomLoginEvent? fromSerialized = JsonSerializer.Deserialize<ExampleCustomLoginEvent>(
      captured.Notice,
      JsonDefaults.DefaultJsonSerializerOptions
    );
    fromSerialized.ShouldBeEquivalentTo(CaseArrangement.ArrangedEvent);
  }

  public class Fixture : NonGenericDispatcher_FromStaticCreate_Fixture
  {
    public Fixture()
    {
      ArrangedEvent = new ExampleCustomLoginEvent
      {
        Username = "test"
      };
      ActResponse = null!;
    }

    public TypedNoticeDispatchResult<ExampleCustomLoginEvent> ActResponse { get; set; }

    public ExampleCustomLoginEvent ArrangedEvent { get; set; }

    protected override async Task ActAsync()
    {
      await base.ActAsync();

      ActResponse = await ArrangedTypedNoticeDispatcher.DispatchAsync(
        ArrangedEvent,
        EventStreamId.From(ArrangedDefaultStream)
      );
    }
  }
}
