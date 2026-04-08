using Jds.NiceNotice.Dispatching.Implementations;
using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Standard;
using Jds.NiceNotice.TypedNotices;
using Jds.NiceNotice.TypedNotices.Routing;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Jds.NiceNotice.Tests.Unit;

public class DelegateRouterTests
{
  [Fact]
  public async Task CanUseDelegateStreamSelector()
  {
    ServiceProvider serviceProvider = new ServiceCollection()
      .AddNiceNotice(builder => builder
        .UseTypedNotices(
          typedNoticeBuilder => typedNoticeBuilder.RouteWithDelegate(enterpriseEvent =>
            EventStreamId.From(enterpriseEvent.GetType().Name.ToUpperInvariant())
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

    response.IoRequest.Stream.ShouldBe(EventStreamId.From(notice.GetType().Name.ToUpperInvariant()));
    capturingNoticeIo.CapturedNotices.ShouldContain(tuple =>
      tuple.Stream == EventStreamId.From(notice.GetType().Name.ToUpperInvariant())
    );
  }
}
