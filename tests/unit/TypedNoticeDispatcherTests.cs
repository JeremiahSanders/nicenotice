using System.Text.Json;

using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Custom;
using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Standard;
using Jds.TestingUtils.Randomization;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

using Xunit.Abstractions;

namespace Jds.NiceNotice.Tests.Unit;

public class TypedNoticeDispatcherTests(ITestOutputHelper outputHelper)
{
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
      item.Item1 == (EventStreamId)defaultStream && item.Item2.Contains(baseNotice.Id.ToString())
    );

    (EventStreamId eventStreamId, string serializedBaseNotice) = dispatchStore.CapturedNotices
      .Single(item =>
        item.Item1 == (EventStreamId)defaultStream && item.Item2.Contains(baseNotice.Id.ToString())
      );
    EnterpriseEvent? baseNoticeFromSerialized = JsonSerializer.Deserialize<EnterpriseEvent>(
      serializedBaseNotice,
      JsonDefaults.DefaultJsonSerializerOptions
    );
    baseNoticeFromSerialized.ShouldBeEquivalentTo(baseNotice);
  }

  [Fact]
  public async Task UsingDefaultInitializer_NonGenericDispatchAsync_DispatchesExpectedContent()
  {
    CapturingNoticeIo dispatcher = new();
    ServiceProvider services = new ServiceCollection()
      .AddNiceNotice(builder => builder.UseDispatcher(services => dispatcher, ServiceLifetime.Singleton))
      .BuildServiceProvider();
    ITypedNoticeDispatcher nonGenericTypedDispatcher = services.GetRequiredService<ITypedNoticeDispatcher>();

    ExampleLogoutEnterpriseEvent message = new()
    {
      Username = Randomizer.Shared.RandomStringLatin(length: 24)
    };

    TypedNoticeDispatchResult<ExampleLogoutEnterpriseEvent> response =
      await nonGenericTypedDispatcher.DispatchAsync(message);

    // Should return the same notice that was sent.
    response.Notice.ShouldBeEquivalentTo(message);
    // Should serialize to JSON by default
    response.Serialized.ShouldNotBeNullOrWhiteSpace();
    ExampleLogoutEnterpriseEvent parsedFromJson = response.DeserializeIoResponseAsJson();
    parsedFromJson.ShouldBeEquivalentTo(message);
    // Should send to a stream having the type's name
    response.Stream.ShouldBe(EventStreamId.From(nameof(ExampleLogoutEnterpriseEvent)));
    // Should send it to the configured dispatcher.
    dispatcher.CapturedNotices.ShouldContain(item =>
      item.Item1 == response.Stream && item.Item2 == response.IoResponse
    );
  }

  [Fact]
  public async Task UsingDefaultInitializer_NonGenericDispatchAsync_WithFullName_DispatchesExpectedContent()
  {
    CapturingNoticeIo dispatcher = new();
    ServiceProvider services = new ServiceCollection()
      .AddNiceNotice(builder => builder.UseDispatcher(services => dispatcher, ServiceLifetime.Singleton))
      .BuildServiceProvider();
    ITypedNoticeDispatcher nonGenericTypedDispatcher = services.GetRequiredService<ITypedNoticeDispatcher>();

    ExampleLogoutEnterpriseEvent message = new()
    {
      Username = Randomizer.Shared.RandomStringLatin(length: 24)
    };

    TypedNoticeDispatchResult<ExampleLogoutEnterpriseEvent> response =
      await nonGenericTypedDispatcher.DispatchAsync(message, dispatchToFullNameStream: true);

    // Should return the same notice that was sent.
    response.Notice.ShouldBeEquivalentTo(message);
    // Should serialize to JSON by default
    response.Serialized.ShouldNotBeNullOrWhiteSpace();
    ExampleLogoutEnterpriseEvent parsedFromJson = response.DeserializeIoResponseAsJson();
    parsedFromJson.ShouldBeEquivalentTo(message);
    // Should send to a stream having the type's name
    response.Stream.ShouldBe(EventStreamId.From(typeof(ExampleLogoutEnterpriseEvent).FullName!));
    // Should send it to the configured dispatcher.
    dispatcher.CapturedNotices.ShouldContain(item =>
      item.Item1 == response.Stream && item.Item2 == response.IoResponse
    );
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
      item.Item1 == (EventStreamId)defaultStream && item.Item2.Contains(messageNotice.Id.ToString())
    );

    (EventStreamId eventStreamId, string serializedBaseNotice) = dispatchStore.CapturedNotices
      .Single(item =>
        item.Item1 == (EventStreamId)defaultStream && item.Item2.Contains(messageNotice.Id.ToString())
      );
    ExampleLogoutEnterpriseEvent? baseNoticeFromSerialized = JsonSerializer.Deserialize<ExampleLogoutEnterpriseEvent>(
      serializedBaseNotice,
      JsonDefaults.DefaultJsonSerializerOptions
    );
    baseNoticeFromSerialized.ShouldBeEquivalentTo(messageNotice);
  }

  [Fact]
  public async Task NonGenericDispatcher_DispatchAsync_DispatchesExpectedContent()
  {
    string defaultStream = Guid.NewGuid().ToString();
    CapturingNoticeIo noticeIo = new();
    TypedNoticeDispatcher ee = TypedNoticeDispatcher.Create(noticeIo);

    ExampleCustomLoginEvent toDispatch = new()
    {
      Username = "test"
    };

    // Act
    TypedNoticeDispatchResult<ExampleCustomLoginEvent> result = await ee.DispatchAsync(
      toDispatch,
      EventStreamId.From(defaultStream)
    );

    OutputNotices(noticeIo.CapturedNotices);

    // Assert
    result.Notice.ShouldBeEquivalentTo(toDispatch);
    noticeIo.CapturedNotices.ShouldContain(item =>
      item.Item1 == (EventStreamId)defaultStream && item.Item2.Contains(toDispatch.Username)
    );
    (EventStreamId eventStreamId, string serialized) = noticeIo.CapturedNotices.Single(item =>
      item.Item1 == (EventStreamId)defaultStream && item.Item2.Contains(toDispatch.Username)
    );
    ExampleCustomLoginEvent? fromSerialized = JsonSerializer.Deserialize<ExampleCustomLoginEvent>(
      serialized,
      JsonDefaults.DefaultJsonSerializerOptions
    );
    fromSerialized.ShouldBeEquivalentTo(toDispatch);
  }

  [Fact]
  public async Task GenericDispatcher_DispatchAsync_DispatchesExpectedContent()
  {
    string defaultStream = Guid
      .NewGuid()
      .ToString();
    EventStreamId defaultStreamId = (EventStreamId)defaultStream;
    CapturingNoticeIo noticeIo = new();
    TypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent> ee =
      TypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent>.Create(
        noticeIo,
        StreamSelectors.Constant<ExampleCustomBaseEnterpriseEvent>(defaultStreamId)
      );

    ExampleCustomLoginEvent toDispatch = new()
    {
      Username = "test"
    };

    // Act
    TypedNoticeDispatchResult<ExampleCustomLoginEvent> result = await ee.DispatchAsync(toDispatch);

    OutputNotices(noticeIo.CapturedNotices);

    // Assert
    result.Notice.ShouldBeEquivalentTo(toDispatch);
    noticeIo.CapturedNotices.ShouldContain(item =>
      item.Item1 == (EventStreamId)defaultStream && item.Item2.Contains(toDispatch.Username)
    );
    (EventStreamId eventStreamId, string serialized) = noticeIo.CapturedNotices.Single(item =>
      item.Item1 == (EventStreamId)defaultStream && item.Item2.Contains(toDispatch.Username)
    );
    ExampleCustomLoginEvent? fromSerialized = JsonSerializer.Deserialize<ExampleCustomLoginEvent>(
      serialized,
      JsonDefaults.DefaultJsonSerializerOptions
    );
    fromSerialized.ShouldBeEquivalentTo(toDispatch);
  }

  private void OutputNotices(IEnumerable<(EventStreamId, string)> capturedNotices)
  {
    outputHelper.WriteLine(message: "Captured notices:");
    foreach ((EventStreamId, string) notice in capturedNotices)
    {
      outputHelper.WriteLine($"{notice.Item1}: {notice.Item2}");
    }

    outputHelper.WriteLine(message: "----END NOTICES----");
  }
}
