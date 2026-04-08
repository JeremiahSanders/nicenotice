using Jds.NiceNotice.Dispatching.Implementations;
using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Standard;
using Jds.NiceNotice.Tests.Unit.ServiceArrangementExamples;
using Jds.NiceNotice.TypedNotices;
using Jds.NiceNotice.TypedNotices.Validation;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Jds.NiceNotice.Tests.Unit;

public class ValidationTests
{
  [Fact]
  public async Task GenericDispatcher_RejectsValidationErrors()
  {
    CapturingNoticeIo noticeIo = new();
    ServiceProvider provider = new ServiceCollection()
      .ApplyConfigurationObjectConfiguration(_ => noticeIo)
      .BuildServiceProvider();

    // NOTE: ExampleLoginEnterpriseEvent has validation rules.
    ITypedNoticeDispatcher<EnterpriseEvent> dispatcher =
      provider.GetRequiredService<ITypedNoticeDispatcher<EnterpriseEvent>>();

    ExampleLoginEnterpriseEvent shouldFail = new()
    {
      Username = string.Empty
    };
    const string expectedFailureMessage =
      $"{nameof(ExampleLoginEnterpriseEvent.Username)}: The {nameof(ExampleLoginEnterpriseEvent.Username)} field is required.";
    const string expectedExceptionMessage =
      $"{nameof(ExampleLoginEnterpriseEvent)} validation failed. " + expectedFailureMessage;

    TypedNoticeDispatchResult<ExampleLoginEnterpriseEvent> result = await dispatcher.DispatchAsync(shouldFail);

    result.IsSuccessful.ShouldBeFalse();
    NoticeValidationException validationException =
      result.ShouldNotBeNull().Exception.ShouldNotBeNull().ShouldBeOfType<NoticeValidationException>();
    validationException.ValidationFailures.ShouldContain(expectedFailureMessage);
    validationException.Message.ShouldBe(expectedExceptionMessage);
  }

  [Fact]
  public async Task NonGenericDispatcher_RejectsValidationErrors()
  {
    CapturingNoticeIo noticeIo = new();
    ServiceProvider provider = new ServiceCollection()
      .ApplyConfigurationObjectConfiguration(_ => noticeIo)
      .BuildServiceProvider();

    ITypedNoticeDispatcher dispatcher = provider.GetRequiredService<ITypedNoticeDispatcher>();

    // NOTE: ExampleLoginEnterpriseEvent has validation rules.
    EventStreamId eventStreamId = EventStreamId.From(value: "example-stream-id");
    ExampleLoginEnterpriseEvent shouldFail = new()
    {
      Username = string.Empty
    };
    const string expectedFailureMessage =
      $"{nameof(ExampleLoginEnterpriseEvent.Username)}: The {nameof(ExampleLoginEnterpriseEvent.Username)} field is required.";
    const string expectedExceptionMessage =
      $"{nameof(ExampleLoginEnterpriseEvent)} validation failed. " + expectedFailureMessage;


    TypedNoticeDispatchResult<ExampleLoginEnterpriseEvent> result = await dispatcher.DispatchAsync(
      shouldFail,
      eventStreamId
    );


    result.IsSuccessful.ShouldBeFalse();
    NoticeValidationException validationException =
      result.ShouldNotBeNull().Exception.ShouldNotBeNull().ShouldBeOfType<NoticeValidationException>();
    validationException.ValidationFailures.ShouldContain(expectedFailureMessage);
    validationException.Message.ShouldBe(expectedExceptionMessage);
  }
}
