using Jds.NiceNotice.Dispatching.Implementations;
using Jds.NiceNotice.Tests.Unit.ExampleApplication;
using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Custom;
using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Standard;
using Jds.NiceNotice.Tests.Unit.ServiceArrangementExamples;
using Jds.NiceNotice.TypedNotices;
using Jds.TestingUtils.Randomization;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

using Xunit.Abstractions;

namespace Jds.NiceNotice.Tests.Unit;

public class ServiceArrangementTests(ITestOutputHelper testOutputHelper)
{
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
    ITypedNoticeDispatcher<EnterpriseEvent> fromExplicitType =
      provider.GetRequiredService<ITypedNoticeDispatcher<EnterpriseEvent>>();
    fromExplicitType.ShouldNotBeNull();

    ITypedNoticeDispatcher nonGenericExplicitType =
      provider.GetRequiredService<ITypedNoticeDispatcher>();
    nonGenericExplicitType.ShouldNotBeNull();

    INoticeIo baseDispatcher = provider.GetRequiredService<INoticeIo>();
    baseDispatcher.ShouldNotBeNull();
  }

  public abstract class BaseServiceArrangementTests(ITestOutputHelper testOutputHelper)
  {
    protected abstract IServiceProvider ArrangeServices(IServiceCollection services);

    [Fact]
    public void CanRegisterAndResolveServices()
    {
      // Act
      IServiceProvider provider = ArrangeServices(new ServiceCollection());

      // Assert
      ITypedNoticeDispatcher<EnterpriseEvent> fromExplicitType =
        provider.GetRequiredService<ITypedNoticeDispatcher<EnterpriseEvent>>();
      fromExplicitType.ShouldNotBeNull();

      ITypedNoticeDispatcher nonGenericExplicitType =
        provider.GetRequiredService<ITypedNoticeDispatcher>();
      nonGenericExplicitType.ShouldNotBeNull();

      INoticeIo baseDispatcher = provider.GetRequiredService<INoticeIo>();
      baseDispatcher.ShouldNotBeNull();
    }

    /// <summary>
    ///   These tests verify that when NiceNotice is added using the builder action overload and not configured
    ///   that it can still be used to dispatch events.
    ///   The expectation is that it uses the <see cref="NullNoticeIo" /> implementation.
    /// </summary>
    [Fact]
    public async Task DispatchingEventsSucceeds_GenericTypedNoticeDispatcher()
    {
      EventStreamId testStreamId = EventStreamId.From(value: "things");
      EnterpriseEvent notice = new();

      // Act
      IServiceProvider provider = ArrangeServices(new ServiceCollection());

      // Assert
      ITypedNoticeDispatcher<EnterpriseEvent> fromExplicitType =
        provider.GetRequiredService<ITypedNoticeDispatcher<EnterpriseEvent>>();
      TypedNoticeDispatchResult<EnterpriseEvent> fromExplicitTypeResult =
        await fromExplicitType.DispatchAsync(notice);
      fromExplicitTypeResult.Serialized.ShouldNotBeNullOrWhiteSpace();
      fromExplicitTypeResult.IoResponse.ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>
    ///   These tests verify that when NiceNotice is added using the builder action overload and not configured
    ///   that it can still be used to dispatch events.
    ///   The expectation is that it uses the <see cref="NullNoticeIo" /> implementation.
    /// </summary>
    [Fact]
    public async Task DispatchingEventsSucceeds_ITypedNoticeDispatcher()
    {
      EventStreamId testStreamId = EventStreamId.From(value: "things");
      EnterpriseEvent notice = new();

      // Act
      IServiceProvider provider = ArrangeServices(new ServiceCollection());

      // Assert
      ITypedNoticeDispatcher nonGenericExplicitType =
        provider.GetRequiredService<ITypedNoticeDispatcher>();
      TypedNoticeDispatchResult<EnterpriseEvent> nonGenericExplicitTypeResult =
        await nonGenericExplicitType.DispatchAsync(notice, testStreamId);
      nonGenericExplicitTypeResult.Serialized.ShouldNotBeNullOrWhiteSpace();
      nonGenericExplicitTypeResult.IoResponse.ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>
    ///   These tests verify that when NiceNotice is added using the builder action overload and not configured
    ///   that it can still be used to dispatch events.
    ///   The expectation is that it uses the <see cref="NullNoticeIo" /> implementation.
    /// </summary>
    [Fact]
    public async Task DispatchingEventsSucceeds_INoticeIo()
    {
      EventStreamId testStreamId = EventStreamId.From(value: "things");
      EnterpriseEvent notice = new();

      // Act
      IServiceProvider provider = ArrangeServices(new ServiceCollection());

      // Assert

      INoticeIo baseDispatcher = provider.GetRequiredService<INoticeIo>();
      string baseDispatcherResult = await baseDispatcher.DispatchAsync(testStreamId, notice.ToString());
      baseDispatcherResult.ShouldBe(notice.ToString());
    }
  }

  public class DefaultFuncWithoutConfiguration(ITestOutputHelper testOutputHelper)
    : BaseServiceArrangementTests(testOutputHelper)
  {
    protected override IServiceProvider ArrangeServices(IServiceCollection services)
    {
      return services
        .AddNiceNotice(builder => { })
        .BuildServiceProvider();
    }
  }

  public class ComprehensiveConfiguration(ITestOutputHelper testOutputHelper)
    : BaseServiceArrangementTests(testOutputHelper)
  {
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    protected override IServiceProvider ArrangeServices(IServiceCollection services)
    {
      return new ServiceCollection()
        .ApplyComprehensiveCustomConfiguration(_testOutputHelper)
        .BuildServiceProvider();
    }
  }

  public class ConfigurationObject(ITestOutputHelper testOutputHelper) : BaseServiceArrangementTests(testOutputHelper)
  {
    protected override IServiceProvider ArrangeServices(IServiceCollection services)
    {
      return new ServiceCollection()
        .ApplyConfigurationObjectConfiguration(serviceProvider => new CapturingNoticeIo())
        .BuildServiceProvider();
    }
  }

  public class HelperConfiguration(ITestOutputHelper testOutputHelper)
  {
    [Fact]
    public void WithHelperBasedConfiguration_CanRegisterAndResolveServices()
    {
      // Act
      ServiceProvider provider = new ServiceCollection()
        .ApplyHelperBasedConfiguration(
          serviceProvider => new ExampleCustomDispatcher(testOutputHelper),
          useFullTypeName: false
        )
        .BuildServiceProvider();

      // Assert
      ITypedNoticeDispatcher<EnterpriseEvent> fromExplicitType =
        provider.GetRequiredService<ITypedNoticeDispatcher<EnterpriseEvent>>();
      fromExplicitType.ShouldNotBeNull();

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
        .ApplyHelperBasedConfiguration(serviceProvider => new CapturingNoticeIo(), useFullTypeName: true)
        .BuildServiceProvider();
      ITypedNoticeDispatcher<EnterpriseEvent> dispatcher =
        provider.GetRequiredService<ITypedNoticeDispatcher<EnterpriseEvent>>();
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
        typeof(ExampleLoginEnterpriseEvent).FullName,
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
  }
}
