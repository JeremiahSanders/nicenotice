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


    Func<Task<TypedNoticeDispatchResult<ExampleLoginEnterpriseEvent>>> act = () => dispatcher.DispatchAsync(shouldFail);

    await act.ShouldThrowAsync<NoticeValidationException>();
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


    Func<Task<TypedNoticeDispatchResult<ExampleLoginEnterpriseEvent>>> act = () =>
      dispatcher.DispatchAsync(shouldFail, eventStreamId);

    await act.ShouldThrowAsync<NoticeValidationException>();
  }
}
