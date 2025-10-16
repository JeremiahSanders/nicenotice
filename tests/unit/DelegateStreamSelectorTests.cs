using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Standard;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Jds.NiceNotice.Tests.Unit;

public class DelegateStreamSelectorTests
{
  [Fact]
  public async Task CanUseDelegateStreamSelector()
  {
    ServiceProvider serviceProvider = new ServiceCollection()
      .AddNiceNotice(builder => builder
        .UseTypedNotices(
          typedNoticeBuilder => typedNoticeBuilder.RouteWithDelegate(enterpriseEvent =>
            EventStreamId.From(enterpriseEvent.Schema.ToUpperInvariant())
          ),
          ServiceLifetime.Transient
        )
        .UseDispatcher(_ => new CapturingNoticeIo(), ServiceLifetime.Singleton)
      )
      .BuildServiceProvider();
    ITypedNoticeDispatcher<EnterpriseEvent> dispatcher =
      serviceProvider.GetRequiredService<ITypedNoticeDispatcher<EnterpriseEvent>>();
    CapturingNoticeIo capturingNoticeIo = serviceProvider.GetRequiredService<INoticeIo>() as CapturingNoticeIo ??
                                          throw new NullReferenceException();
    ExampleLoginEnterpriseEvent notice = new()
    {
      Username = "person"
    };

    TypedNoticeDispatchResult<ExampleLoginEnterpriseEvent> response = await dispatcher.DispatchAsync(notice);

    response.Stream.ShouldBe(EventStreamId.From(notice.Schema.ToUpperInvariant()));
    capturingNoticeIo.CapturedNotices.ShouldContain(tuple =>
      tuple.Item1 == EventStreamId.From(notice.Schema.ToUpperInvariant())
    );
  }
}
