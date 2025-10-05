using Jds.NiceNotice.Tests.Unit.ExampleApplication;
using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Custom;
using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Standard;
using Jds.NiceNotice.Tests.Unit.ServiceArrangementExamples;
using Jds.TestingUtils.Randomization;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

using Xunit.Abstractions;

namespace Jds.NiceNotice.Tests.Unit;

public class ServiceArrangementTests(ITestOutputHelper testOutputHelper)
{
  [Fact]
  public void WithComprehensiveConfiguration_CanRegisterAndResolveServices()
  {
    // Act
    ServiceProvider provider = new ServiceCollection()
      .ApplyComprehensiveCustomConfiguration(testOutputHelper)
      .BuildServiceProvider();

    // Assert
    ITypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent> fromExplicitType =
      provider.GetRequiredService<ITypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent>>();
    fromExplicitType.ShouldNotBeNull();

    ITypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent> fromExtension =
      provider.GetEnterpriseEventDispatcher<ExampleCustomBaseEnterpriseEvent>();
    fromExtension.ShouldNotBeNull();

    ITypedNoticeDispatcher nonGenericExplicitType =
      provider.GetRequiredService<ITypedNoticeDispatcher>();
    nonGenericExplicitType.ShouldNotBeNull();

    INoticeIo baseDispatcher = provider.GetRequiredService<INoticeIo>();
    baseDispatcher.ShouldNotBeNull();
  }

  [Fact]
  public void WithHelperBasedConfiguration_CanRegisterAndResolveServices()
  {
    // Act
    ServiceProvider provider = new ServiceCollection()
      .ApplyHelperBasedConfiguration(testOutputHelper, serviceProvider => new ExampleCustomDispatcher(testOutputHelper))
      .BuildServiceProvider();

    // Assert
    ITypedNoticeDispatcher<EnterpriseEventBase> fromExplicitType =
      provider.GetRequiredService<ITypedNoticeDispatcher<EnterpriseEventBase>>();
    fromExplicitType.ShouldNotBeNull();

    ITypedNoticeDispatcher<EnterpriseEventBase> fromExtension =
      provider.GetEnterpriseEventDispatcher<EnterpriseEventBase>();
    fromExtension.ShouldNotBeNull();

    ITypedNoticeDispatcher nonGenericExplicitType =
      provider.GetRequiredService<ITypedNoticeDispatcher>();
    nonGenericExplicitType.ShouldNotBeNull();

    INoticeIo baseDispatcher = provider.GetRequiredService<INoticeIo>();
    baseDispatcher.ShouldNotBeNull();
  }

  [Fact]
  public async Task WithHelperBasedConfiguration_CanDispatchEvents()
  {
    // Arrange
    ExampleLoginEnterpriseEvent customLoginEvent = new()
    {
      Username = $"{Randomizer.Shared.DemographicsSurnameUsa()}.{Randomizer.Shared.DemographicsForenameUsa()}"
    };
    ServiceProvider provider = new ServiceCollection()
      .ApplyHelperBasedConfiguration(testOutputHelper, serviceProvider => new CapturingNoticeIo())
      .BuildServiceProvider();
    ITypedNoticeDispatcher<EnterpriseEventBase> dispatcher =
      provider.GetRequiredService<ITypedNoticeDispatcher<EnterpriseEventBase>>();
    CapturingNoticeIo destination = provider.GetRequiredService<INoticeIo>() as CapturingNoticeIo ??
                                    throw new NullReferenceException();

    // Act
    TypedNoticeDispatchResult<ExampleLoginEnterpriseEvent> response = await dispatcher.DispatchAsync(
      customLoginEvent
    );

    // Assert
    // Check the body of the response
    response.ShouldNotBeNull();
    ((string)response.Stream).ShouldBe(
      nameof(ExampleLoginEnterpriseEvent),
      customMessage: "This arrangement should be using type name streams."
    );
    response.IoResponse.ShouldNotBeNullOrWhiteSpace();
    ExampleLoginEnterpriseEvent deserialized = response.DeserializeIoResponseAsJson();
    deserialized.ShouldBeEquivalentTo(customLoginEvent);
    // Now check our I/O captures
    destination.CapturedNotices.ShouldContain(tuple =>
      tuple.Item1 == response.Stream && tuple.Item2 == response.IoResponse
    );
  }


  [Fact]
  public void WithCustomBaseEvent_CanRegisterAndResolveServices()
  {
    IServiceCollection services = new ServiceCollection();

    // Act
    services.AddNiceNotice(builder => builder
      .UseTypedNotices<ExampleCustomBaseEnterpriseEvent>(
        static eeBuilder => { },
        ServiceLifetime.Singleton
      )
    );

    ServiceProvider provider = services.BuildServiceProvider();

    // Assert
    ITypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent> fromExplicitType =
      provider.GetRequiredService<ITypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent>>();
    fromExplicitType.ShouldNotBeNull();

    ITypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent> fromExtension =
      provider.GetEnterpriseEventDispatcher<ExampleCustomBaseEnterpriseEvent>();
    fromExtension.ShouldNotBeNull();

    ITypedNoticeDispatcher nonGenericExplicitType =
      provider.GetRequiredService<ITypedNoticeDispatcher>();
    nonGenericExplicitType.ShouldNotBeNull();

    INoticeIo baseDispatcher = provider.GetRequiredService<INoticeIo>();
    baseDispatcher.ShouldNotBeNull();
  }

  [Fact]
  public void WithDefaultBaseEvent_CanRegisterAndResolveServices()
  {
    IServiceCollection services = new ServiceCollection();

    // Act
    services.AddNiceNotice(builder => builder
      .UseTypedNotices(
        eeBuilder => { },
        ServiceLifetime.Singleton
      )
    );

    ServiceProvider provider = services.BuildServiceProvider();

    // Assert
    ITypedNoticeDispatcher<EnterpriseEventBase> fromExplicitType =
      provider.GetRequiredService<ITypedNoticeDispatcher<EnterpriseEventBase>>();
    fromExplicitType.ShouldNotBeNull();

    ITypedNoticeDispatcher<EnterpriseEventBase> fromExtension =
      provider.GetEnterpriseEventDispatcher<EnterpriseEventBase>();
    fromExtension.ShouldNotBeNull();

    ITypedNoticeDispatcher nonGenericExplicitType =
      provider.GetRequiredService<ITypedNoticeDispatcher>();
    nonGenericExplicitType.ShouldNotBeNull();

    INoticeIo baseDispatcher = provider.GetRequiredService<INoticeIo>();
    baseDispatcher.ShouldNotBeNull();
  }

  [Fact]
  public void WithoutConfiguration_CanRegisterAndResolveServices()
  {
    IServiceCollection services = new ServiceCollection();

    // Act
    services.AddNiceNotice(builder => { });

    ServiceProvider provider = services.BuildServiceProvider();

    // Assert
    ITypedNoticeDispatcher<EnterpriseEventBase> fromExplicitType =
      provider.GetRequiredService<ITypedNoticeDispatcher<EnterpriseEventBase>>();
    fromExplicitType.ShouldNotBeNull();

    ITypedNoticeDispatcher<EnterpriseEventBase> fromExtension =
      provider.GetEnterpriseEventDispatcher<EnterpriseEventBase>();
    fromExtension.ShouldNotBeNull();

    ITypedNoticeDispatcher nonGenericExplicitType =
      provider.GetRequiredService<ITypedNoticeDispatcher>();
    nonGenericExplicitType.ShouldNotBeNull();

    INoticeIo baseDispatcher = provider.GetRequiredService<INoticeIo>();
    baseDispatcher.ShouldNotBeNull();
  }
}
