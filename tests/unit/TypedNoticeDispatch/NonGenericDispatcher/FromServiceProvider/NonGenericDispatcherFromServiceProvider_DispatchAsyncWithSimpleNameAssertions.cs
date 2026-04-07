using System.Net.Mime;

using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Standard;
using Jds.TestingUtils.Xunit2.Extras;

using Shouldly;

namespace Jds.NiceNotice.Tests.Unit.TypedNoticeDispatch.NonGenericDispatcher.FromServiceProvider;

public class NonGenericDispatcherFromServiceProvider_DispatchAsyncWithSimpleNameAssertions(
  NonGenericDispatcherFromServiceProvider_DispatchAsyncWithSimpleNameAssertions.Fixture caseArrangementFixture
)
  : BaseCaseAssertions<NonGenericDispatcherFromServiceProvider_DispatchAsyncWithSimpleNameAssertions.Fixture>(
    caseArrangementFixture
  )
{
  [Fact]
  public void IoRequestHasMetadata()
  {
    CaseArrangement.ActResponse.IoRequest.Metadata.ShouldNotBeNull();
    CaseArrangement.ActResponse.IoRequest.Metadata.ContainsKey(key: "duration").ShouldBeTrue();
  }

  [Fact]
  public void IoRequestSerializesToJsonByDefault()
  {
    CaseArrangement.ActResponse.IoRequest.Notice.ShouldNotBeNullOrWhiteSpace();
    CaseArrangement.ActResponse.IoRequest.ContentType.ShouldBe(MediaTypeNames.Application.Json);
    ExampleLogoutEnterpriseEvent parsedFromJson = CaseArrangement.ActResponse.DeserializeIoResponseAsJson();
    parsedFromJson.ShouldBeEquivalentTo(CaseArrangement.ArrangedMessage);
  }

  [Fact]
  public void IoRequestStreamMatchesDirectTypeName()
  {
    CaseArrangement.ActResponse.IoRequest.Stream.ShouldBe(EventStreamId.From(nameof(ExampleLogoutEnterpriseEvent)));
  }

  [Fact]
  public void ReturnsTheSameNoticeThatWasSent()
  {
    CaseArrangement.ActResponse.Notice.ShouldBeEquivalentTo(CaseArrangement.ArrangedMessage);
  }

  [Fact]
  public void SideEffectValidation_DispatchedMessageHasMetadata()
  {
    CaseArrangement.ArrangedDispatcher.CapturedNotices.ShouldAllBe(capturedNotice =>
      capturedNotice.Metadata != null
      && capturedNotice.Metadata.ContainsKey("duration")
    );
  }

  [Fact]
  public void SideEffectValidation_ShouldSendToTheConfiguredDispatcher()
  {
    CaseArrangement.ArrangedDispatcher.CapturedNotices.ShouldContain(item =>
      item.Stream == CaseArrangement.ActResponse.IoRequest.Stream &&
      item.Notice == CaseArrangement.ActResponse.IoRequest.Notice
    );
  }

  public class Fixture() : NonGenericDispatchAsyncFixture(
    actDispatchToFullNameStream: false
  )
  {
  }
}
