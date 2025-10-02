using System.Text.Json;

using Jds.NiceNotice.Tests.Unit.ExampleApplication;

using Shouldly;

using Xunit.Abstractions;

namespace Jds.NiceNotice.Tests.Unit;

public class TypedNoticeDispatcherTests(ITestOutputHelper outputHelper)
{
  [Fact]
  public async Task NonGenericDispatcher_DispatchAsync_DispatchesExpectedContent()
  {
    string defaultStream = Guid
      .NewGuid()
      .ToString();
    CapturingDispatcher dispatcher = new();
    TypedNoticeDispatcher ee =
      TypedNoticeDispatcher.Create(
        dispatcher
      );

    ExampleLoginEvent toDispatch = new()
    {
      Username = "test"
    };

    // Act
    TypedNoticeDispatchResult<ExampleLoginEvent> result = await ee.DispatchAsync(
      toDispatch,
      EventStreamId.From(defaultStream)
    );

    OutputNotices(dispatcher.CapturedNotices);

    // Assert
    result.Notice.ShouldBeEquivalentTo(toDispatch);
    dispatcher.CapturedNotices.ShouldContain(item =>
      item.Item1 == (EventStreamId)defaultStream && item.Item2.Contains(toDispatch.Username)
    );
    (EventStreamId eventStreamId, string serialized) = dispatcher.CapturedNotices.Single(item =>
      item.Item1 == (EventStreamId)defaultStream && item.Item2.Contains(toDispatch.Username)
    );
    ExampleLoginEvent? fromSerialized = JsonSerializer.Deserialize<ExampleLoginEvent>(
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
    CapturingDispatcher dispatcher = new();
    TypedNoticeDispatcher<ExampleBaseEnterpriseEvent> ee =
      TypedNoticeDispatcher<ExampleBaseEnterpriseEvent>.Create(
        dispatcher,
        StreamSelectors.Constant<ExampleBaseEnterpriseEvent>(defaultStreamId)
      );

    ExampleLoginEvent toDispatch = new()
    {
      Username = "test"
    };

    // Act
    TypedNoticeDispatchResult<ExampleLoginEvent> result = await ee.DispatchAsync(toDispatch);

    OutputNotices(dispatcher.CapturedNotices);

    // Assert
    result.Notice.ShouldBeEquivalentTo(toDispatch);
    dispatcher.CapturedNotices.ShouldContain(item =>
      item.Item1 == (EventStreamId)defaultStream && item.Item2.Contains(toDispatch.Username)
    );
    (EventStreamId eventStreamId, string serialized) = dispatcher.CapturedNotices.Single(item =>
      item.Item1 == (EventStreamId)defaultStream && item.Item2.Contains(toDispatch.Username)
    );
    ExampleLoginEvent? fromSerialized = JsonSerializer.Deserialize<ExampleLoginEvent>(
      serialized,
      JsonDefaults.DefaultJsonSerializerOptions
    );
    fromSerialized.ShouldBeEquivalentTo(toDispatch);
  }

  private void OutputNotices(IEnumerable<(EventStreamId, string)> capturedNotices)
  {
    foreach ((EventStreamId, string) notice in capturedNotices)
    {
      outputHelper.WriteLine($"{notice.Item1}: {notice.Item2}");
    }
  }
}
