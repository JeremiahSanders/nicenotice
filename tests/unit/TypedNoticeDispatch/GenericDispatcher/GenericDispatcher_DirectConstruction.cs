using System.Net.Mime;
using System.Text.Json;

using Jds.NiceNotice.Dispatching;
using Jds.NiceNotice.Dispatching.Implementations;
using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Custom;
using Jds.NiceNotice.TypedNotices;
using Jds.NiceNotice.TypedNotices.Routing;
using Jds.TestingUtils.Randomization;

using Shouldly;

using Xunit.Abstractions;

namespace Jds.NiceNotice.Tests.Unit.TypedNoticeDispatch.GenericDispatcher;

/// <summary>
///   This tests the behavior of a <see cref="TypedNoticeDispatcher{TEnterpriseEventBaseType}" />
///   (the generic version) in isolation
///   (i.e., not related to the <see cref="ServiceCollectionExtensions.AddNiceNotice" />
///   extension method).
/// </summary>
/// <param name="testOutputHelper"></param>
public class GenericDispatcher_DirectConstruction(ITestOutputHelper testOutputHelper)
  : TypedNoticeDispatcherFixture(testOutputHelper)
{
  /// <summary>
  ///   Verifies that I/O failures during single-notice dispatch
  ///   are captured and returned in the result, rather than being thrown.
  /// </summary>
  [Fact]
  public async Task DispatchAsync_CapturesIoFailures()
  {
    string defaultStream = Guid
      .NewGuid()
      .ToString();
    EventStreamId defaultStreamId = (EventStreamId)defaultStream;
    INoticeBatchIo noticeIo = DelegateBatchNoticeIo.AlwaysFails_Batch();
    TypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent> ee =
      TypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent>.Create(
        noticeIo,
        Routers.Constant<ExampleCustomBaseEnterpriseEvent>(defaultStreamId)
      );

    TypedNoticeDispatchResult<ExampleCustomLoginEvent> actual = await ee.DispatchAsync(
      new ExampleCustomLoginEvent
      {
        Username = "test1"
      }
    );

    actual.ShouldNotBeNull();
    actual.IsSuccessful.ShouldBeFalse();
    actual.Exception.ShouldNotBeNull();
  }

  [Fact]
  public async Task DispatchBatchAsync_CapturesBatchIoFailures()
  {
    string defaultStream = Guid
      .NewGuid()
      .ToString();
    EventStreamId defaultStreamId = (EventStreamId)defaultStream;
    INoticeBatchIo noticeIo = DelegateBatchNoticeIo.AlwaysFails_Batch();
    TypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent> ee =
      TypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent>.Create(
        noticeIo,
        Routers.Constant<ExampleCustomBaseEnterpriseEvent>(defaultStreamId)
      );

    // Create some events to dispatch.
    ExampleCustomLoginEvent login1 = new()
    {
      Username = "test1"
    };
    ExampleCustomLoginEvent login2 = new()
    {
      Username = "test2"
    };

    DispatchBatchRequest<ExampleCustomBaseEnterpriseEvent> request =
      DispatchBatchRequest<ExampleCustomBaseEnterpriseEvent>.CreateFromTypedNotices(
        [login1, login2],
        new BatchDispatchOptions
        {
          MaxDegreeOfParallelism = 4
        }
      );

    // Act
    BatchTypedNoticeDispatchResult response = await ee.DispatchBatchAsync(request);

    // Assert
    request.Notices.ShouldAllBe(kvp => response.Failures.Any(tuple => tuple.BatchNoticeId == kvp.Key));
  }

  [Fact]
  public async Task DispatchBatchAsync_CapturesIoFailures()
  {
    string defaultStream = Guid
      .NewGuid()
      .ToString();
    EventStreamId defaultStreamId = (EventStreamId)defaultStream;
    INoticeIo noticeIo = DelegateNoticeIo.AlwaysFails();
    TypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent> ee =
      TypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent>.Create(
        noticeIo,
        Routers.Constant<ExampleCustomBaseEnterpriseEvent>(defaultStreamId)
      );

    // Create some events to dispatch.
    ExampleCustomLoginEvent login1 = new()
    {
      Username = "test1"
    };
    ExampleCustomLoginEvent login2 = new()
    {
      Username = "test2"
    };

    DispatchBatchRequest<ExampleCustomBaseEnterpriseEvent> request =
      DispatchBatchRequest<ExampleCustomBaseEnterpriseEvent>.CreateFromTypedNotices(
        [login1, login2],
        new BatchDispatchOptions
        {
          MaxDegreeOfParallelism = 4
        }
      );

    // Act
    BatchTypedNoticeDispatchResult response = await ee.DispatchBatchAsync(request);

    // Assert
    request.Notices.ShouldAllBe(kvp => response.Failures.Any(tuple => tuple.BatchNoticeId == kvp.Key));
  }

  [Fact]
  public async Task DispatchBatchAsync_DispatchesExpectedContent()
  {
    string defaultStream = Guid
      .NewGuid()
      .ToString();
    EventStreamId defaultStreamId = (EventStreamId)defaultStream;
    CapturingNoticeIo noticeIo = new();
    TypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent> ee =
      TypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent>.Create(
        noticeIo,
        Routers.Constant<ExampleCustomBaseEnterpriseEvent>(defaultStreamId)
      );

    // Create some events to dispatch.
    ExampleCustomLoginEvent login1 = new()
    {
      Username = "test1"
    };
    ExampleCustomLoginEvent login2 = new()
    {
      Username = "test2"
    };
    ExampleCustomLogoutEvent logout3 = new()
    {
      Username = "test3"
    };
    ExampleCustomLogoutEvent logout4 = new()
    {
      Username = "test4"
    };
    DispatchBatchRequest<ExampleCustomBaseEnterpriseEvent> request =
      DispatchBatchRequest<ExampleCustomBaseEnterpriseEvent>.CreateFromTypedNotices(
        [login1, login2, logout3, logout4],
        new BatchDispatchOptions
        {
          MaxDegreeOfParallelism = 2
        }
      );

    // Act
    BatchTypedNoticeDispatchResult result = await ee.DispatchBatchAsync(
      request
    );

    OutputNotices(noticeIo.CapturedNotices);

    // Assert
    //   All the notices should have been returned as a success.
    request.Notices.ShouldAllBe(kvp => result.Successes.Any(response => response.BatchNoticeId == kvp.Key));
    //   The dispatched notices should be returned.
    result.Successes.ShouldAllBe(response =>
      request.Notices[response.BatchNoticeId] == (ExampleCustomBaseEnterpriseEvent)response.TypedNotice
    );
    result.Failures.ShouldBeEmpty();

    AssertNoticeWasDispatched(login1.Username, login1);
    AssertNoticeWasDispatched(login2.Username, login2);
    AssertNoticeWasDispatched(logout3.Username, logout3);
    AssertNoticeWasDispatched(logout4.Username, logout4);

    return;

    void AssertNoticeWasDispatched(string valueToFind, object expected)
    {
      // The notice should have gone to the default stream and it should contain the value to find.
      noticeIo.CapturedNotices.ShouldContain(item =>
        item.Stream == (EventStreamId)defaultStream &&
        item.Notice.Contains(valueToFind)
      );

      // The notice should have been serialized to JSON as expected.
      IoRequestNotice actual = noticeIo.CapturedNotices.First(item =>
        item.Stream == (EventStreamId)defaultStream &&
        item.Notice.Contains(valueToFind)
      );
      string expectedJson = JsonSerializer.Serialize(expected, JsonDefaults.DefaultJsonSerializerOptions);

      actual.Notice.ShouldBe(expectedJson);

      // Dispatched message should have metadata and content type
      noticeIo.CapturedNotices.ShouldAllBe(capturedNotice =>
        capturedNotice.ContentType == MediaTypeNames.Application.Json
      );
      noticeIo.CapturedNotices.ShouldAllBe(capturedNotice =>
        // Only checking schema; Only the logout events also have duration
        capturedNotice.Metadata != null && capturedNotice.Metadata.ContainsKey("name")
      );
    }
  }

  [Fact]
  public async Task DispatchBatchAsync_GivenEnumerableOverload_DispatchesExpectedContent()
  {
    string defaultStream = Guid
      .NewGuid()
      .ToString();
    EventStreamId defaultStreamId = (EventStreamId)defaultStream;
    CapturingNoticeIo noticeIo = new();
    TypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent> ee =
      TypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent>.Create(
        noticeIo,
        Routers.Constant<ExampleCustomBaseEnterpriseEvent>(defaultStreamId)
      );

    // Create some events to dispatch.
    ExampleCustomLoginEvent login1 = new()
    {
      Username = "test1"
    };
    ExampleCustomLoginEvent login2 = new()
    {
      Username = "test2"
    };
    ExampleCustomLogoutEvent logout3 = new()
    {
      Username = "test3"
    };
    ExampleCustomLogoutEvent logout4 = new()
    {
      Username = "test4"
    };
    BatchDispatchOptions options = new()
    {
      MaxDegreeOfParallelism = Randomizer.Shared.IntInRange(minInclusive: 1, maxExclusive: 9)
    };
    List<ExampleCustomBaseEnterpriseEvent> notices = [login1, login2, logout3, logout4];

    // Act
    BatchTypedNoticeDispatchResult result =
      await ee.DispatchBatchAsync(notices, options);

    OutputNotices(noticeIo.CapturedNotices);

    // Assert
    //   All the notices should have been returned as a success.
    notices.ShouldAllBe(kvp => result.Successes.Any(response => ReferenceEquals(response.TypedNotice, kvp)));
    //   The dispatched notices should be returned.
    result.Successes.ShouldAllBe(response => notices.Any(notice => ReferenceEquals(response.TypedNotice, notice)));
    result.Failures.ShouldBeEmpty();

    AssertNoticeWasDispatched(login1.Username, login1);
    AssertNoticeWasDispatched(login2.Username, login2);
    AssertNoticeWasDispatched(logout3.Username, logout3);
    AssertNoticeWasDispatched(logout4.Username, logout4);

    return;

    void AssertNoticeWasDispatched(string valueToFind, object expected)
    {
      // The notice should have gone to the default stream and it should contain the value to find.
      noticeIo.CapturedNotices.ShouldContain(item =>
        item.Stream == (EventStreamId)defaultStream &&
        item.Notice.Contains(valueToFind)
      );

      // The notice should have been serialized to JSON as expected.
      IoRequestNotice actual = noticeIo.CapturedNotices.First(item =>
        item.Stream == (EventStreamId)defaultStream &&
        item.Notice.Contains(valueToFind)
      );
      string expectedJson = JsonSerializer.Serialize(expected, JsonDefaults.DefaultJsonSerializerOptions);

      actual.Notice.ShouldBe(expectedJson);

      // Dispatched message should have metadata and content type
      noticeIo.CapturedNotices.ShouldAllBe(capturedNotice =>
        capturedNotice.ContentType == MediaTypeNames.Application.Json
      );
      noticeIo.CapturedNotices.ShouldAllBe(capturedNotice =>
        // Only checking schema; Only the logout events also have duration
        capturedNotice.Metadata != null && capturedNotice.Metadata.ContainsKey("name")
      );
    }
  }
}
