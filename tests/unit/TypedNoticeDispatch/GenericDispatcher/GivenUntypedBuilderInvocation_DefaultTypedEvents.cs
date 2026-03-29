using System.Net.Mime;
using System.Text.Json;

using Jds.NiceNotice.Dispatching.Implementations;
using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Standard;
using Jds.NiceNotice.TypedNotices;
using Jds.NiceNotice.TypedNotices.Routing;
using Jds.TestingUtils.Randomization;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

using Xunit.Abstractions;

namespace Jds.NiceNotice.Tests.Unit.TypedNoticeDispatch.GenericDispatcher;

/// <summary>
///   Tests verifying use of <see cref="Jds.NiceNotice.Configuration.NiceNoticeBuilder.UseTypedNotices" /> without any
///   type arguments.
///   This is the second-most basic configuration.
///   In this arrangement, we will assume that <see cref="EnterpriseEvent" /> is the default base type.
/// </summary>
/// <param name="testOutputHelper"></param>
public class GivenUntypedBuilderInvocation_DefaultTypedEvents(ITestOutputHelper testOutputHelper)
  : TypedNoticeDispatcherFixture(testOutputHelper)
{
  /// <summary>
  ///   Verify that we can request a generic dispatcher (where the base type is <see cref="EnterpriseEvent" />)
  ///   and that it works as expected.
  /// </summary>
  [Fact]
  public async Task WhenUsingDefaultTypedEvents_CanDispatchABaseNotice()
  {
    string defaultStream = Randomizer.Shared.RandomStringLatin(length: 16);
    ServiceProvider sp = new ServiceCollection()
      .AddNiceNotice(builder => builder
        .UseTypedNotices(
          typedNoticeBuilder => typedNoticeBuilder.RouteToConstantStream((EventStreamId)defaultStream),
          ServiceLifetime.Transient
        )
        .UseDispatcher<CapturingNoticeIo>(ServiceLifetime.Singleton)
      )
      .BuildServiceProvider();
    CapturingNoticeIo dispatchStore = sp.GetRequiredService<CapturingNoticeIo>();
    ITypedNoticeDispatcher<EnterpriseEvent> typedDispatcher =
      sp.GetRequiredService<ITypedNoticeDispatcher<EnterpriseEvent>>();

    EnterpriseEvent baseNotice = new();

    // Act
    TypedNoticeDispatchResult<EnterpriseEvent> typedResponseBaseNotice =
      await typedDispatcher.DispatchAsync(baseNotice);

    OutputNotices(dispatchStore.CapturedNotices);

    // Assert
    typedResponseBaseNotice.Notice.ShouldBeEquivalentTo(baseNotice);

    dispatchStore.CapturedNotices.ShouldContain(item =>
      item.Stream == (EventStreamId)defaultStream && item.Notice.Contains(baseNotice.Id.ToString())
    );

    IoRequestNotice capturedBaseNotice = dispatchStore.CapturedNotices
      .Single(item =>
        item.Stream == (EventStreamId)defaultStream && item.Notice.Contains(baseNotice.Id.ToString())
      );
    EnterpriseEvent? baseNoticeFromSerialized = JsonSerializer.Deserialize<EnterpriseEvent>(
      capturedBaseNotice.Notice,
      JsonDefaults.DefaultJsonSerializerOptions
    );
    baseNoticeFromSerialized.ShouldBeEquivalentTo(baseNotice);
  }

  [Fact]
  public async Task WhenUsingDefaultTypedEvents_CanDispatchMessageNotice()
  {
    string defaultStream = Randomizer.Shared.RandomStringLatin(length: 16);
    ServiceProvider sp = new ServiceCollection()
      .AddNiceNotice(builder => builder
        .UseTypedNotices(
          typedNoticeBuilder => typedNoticeBuilder.RouteToConstantStream((EventStreamId)defaultStream),
          ServiceLifetime.Transient
        )
        .UseDispatcher<CapturingNoticeIo>(ServiceLifetime.Singleton)
      )
      .BuildServiceProvider();
    CapturingNoticeIo dispatchStore = sp.GetRequiredService<CapturingNoticeIo>();
    ITypedNoticeDispatcher<EnterpriseEvent> typedDispatcher =
      sp.GetRequiredService<ITypedNoticeDispatcher<EnterpriseEvent>>();

    ExampleLogoutEnterpriseEvent messageNotice = new()
    {
      Username = Randomizer.Shared.RandomStringLatin(length: 12)
    };

    // Act
    TypedNoticeDispatchResult<ExampleLogoutEnterpriseEvent> typedResponseMessageNotice =
      await typedDispatcher.DispatchAsync(messageNotice);

    OutputNotices(dispatchStore.CapturedNotices);

    // Assert
    typedResponseMessageNotice.Notice.ShouldBeEquivalentTo(messageNotice);

    dispatchStore.CapturedNotices.ShouldContain(item =>
      item.Stream == (EventStreamId)defaultStream && item.Notice.Contains(messageNotice.Id.ToString())
    );

    IoRequestNotice capturedBaseNotice = dispatchStore.CapturedNotices
      .Single(item =>
        item.Stream == (EventStreamId)defaultStream && item.Notice.Contains(messageNotice.Id.ToString())
      );
    ExampleLogoutEnterpriseEvent? baseNoticeFromSerialized = JsonSerializer.Deserialize<ExampleLogoutEnterpriseEvent>(
      capturedBaseNotice.Notice,
      JsonDefaults.DefaultJsonSerializerOptions
    );
    baseNoticeFromSerialized.ShouldBeEquivalentTo(messageNotice);
    // Dispatched message should have metadata and content type
    dispatchStore.CapturedNotices.ShouldAllBe(capturedNotice =>
      capturedNotice.ContentType == MediaTypeNames.Application.Json
    );
    dispatchStore.CapturedNotices.ShouldAllBe(capturedNotice =>
      capturedNotice.Metadata != null && capturedNotice.Metadata.ContainsKey("schema") &&
      capturedNotice.Metadata.ContainsKey("duration")
    );
  }
}
