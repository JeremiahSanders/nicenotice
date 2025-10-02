using Jds.NiceNotice.Tests.Unit.ExampleApplication;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Jds.NiceNotice.Tests.Unit;

public class ServiceArrangementTests
{
  [Fact]
  public void CanRegisterAndResolveServices()
  {
    IServiceCollection services = new ServiceCollection();

    // Act
    services.AddNiceNotice(builder => builder
      .UseTypedNotices<ExampleBaseEnterpriseEvent>(
        eeBuilder => { },
        ServiceLifetime.Singleton
      )
    );

    ServiceProvider provider = services.BuildServiceProvider();

    // Assert
    ITypedNoticeDispatcher<ExampleBaseEnterpriseEvent> fromExplicitType =
      provider.GetRequiredService<ITypedNoticeDispatcher<ExampleBaseEnterpriseEvent>>();
    fromExplicitType.ShouldNotBeNull();

    ITypedNoticeDispatcher<ExampleBaseEnterpriseEvent> fromExtension =
      provider.GetEnterpriseEventDispatcher<ExampleBaseEnterpriseEvent>();
    fromExtension.ShouldNotBeNull();

    ITypedNoticeDispatcher nonGenericExplicitType =
      provider.GetRequiredService<ITypedNoticeDispatcher>();
    nonGenericExplicitType.ShouldNotBeNull();

    INoticeIo baseDispatcher = provider.GetRequiredService<INoticeIo>();
    baseDispatcher.ShouldNotBeNull();
  }
}
