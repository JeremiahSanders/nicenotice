using System.Net.Mime;
using System.Text.Json;

using Jds.NiceNotice.Dispatching;
using Jds.NiceNotice.Dispatching.Implementations;
using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Custom;
using Jds.NiceNotice.TypedNotices;
using Jds.TestingUtils.Randomization;

using Shouldly;

using Xunit.Abstractions;

namespace Jds.NiceNotice.Tests.Unit.TypedNoticeDispatch.NonGenericDispatcher.FromStaticCreate;

/// <summary>
///   This tests the behavior of a <see cref="TypedNoticeDispatcher" /> (the non-generic version)
///   in isolation (i.e., not related to the <see cref="ServiceCollectionExtensions.AddNiceNotice" />
///   extension method).
/// </summary>
/// <param name="testOutputHelper"></param>
public class NonGenericDispatcher_DirectConstruction(ITestOutputHelper testOutputHelper)
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
    TypedNoticeDispatcher ee = TypedNoticeDispatcher.Create(noticeIo);

    TypedNoticeDispatchResult<ExampleCustomLoginEvent> result = await ee.DispatchAsync(
      new ExampleCustomLoginEvent
      {
        Username = "test1"
      }
    );

    result.ShouldNotBeNull();
    result.IsSuccessful.ShouldBeFalse();
    result.Exception.ShouldNotBeNull();
  }

  [Fact]
  public async Task DispatchBatchAsync_CapturesBatchIoFailures()
  {
    string defaultStream = Guid
      .NewGuid()
      .ToString();
    EventStreamId defaultStreamId = (EventStreamId)defaultStream;
    INoticeBatchIo noticeIo = DelegateBatchNoticeIo.AlwaysFails_Batch();
    TypedNoticeDispatcher ee = TypedNoticeDispatcher.Create(noticeIo);

    // Create some events to dispatch.
    ExampleCustomLoginEvent login1 = new()
    {
      Username = "test1"
    };
    ExampleCustomLoginEvent login2 = new()
    {
      Username = "test2"
    };

    BatchDispatchRequest request = BatchDispatchRequest.CreateForSingleStream(
      defaultStreamId,
      [login1, login2],
      options: new BatchDispatchOptions
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
    TypedNoticeDispatcher ee = TypedNoticeDispatcher.Create(noticeIo);

    // Create some events to dispatch.
    ExampleCustomLoginEvent login1 = new()
    {
      Username = "test1"
    };
    ExampleCustomLoginEvent login2 = new()
    {
      Username = "test2"
    };

    BatchDispatchRequest request = BatchDispatchRequest.CreateForSingleStream(
      defaultStreamId,
      [login1, login2],
      options: new BatchDispatchOptions
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
    TypedNoticeDispatcher ee = TypedNoticeDispatcher.Create(noticeIo);

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
    BatchDispatchRequest request = BatchDispatchRequest.CreateForSingleStream(
      defaultStreamId,
      [
        login1, login2, logout3, logout4
      ],
      options:
      new BatchDispatchOptions
      {
        MaxDegreeOfParallelism = 2
      }
    );

    // Act
    BatchTypedNoticeDispatchResult result = await ee.DispatchBatchAsync(request);

    OutputNotices(noticeIo.CapturedNotices);

    // Assert
    //   All the notices should have been returned as a success.
    request.Notices.ShouldAllBe(kvp => result.Successes.Any(response => response.BatchNoticeId == kvp.Key));
    //   The dispatched notices should be returned.
    result.Successes.ShouldAllBe(response =>
      request.Notices[response.BatchNoticeId].Notice == response.TypedNotice
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
      IoNoticeDispatchRequest actual = noticeIo.CapturedNotices.First(item =>
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
  public async Task DispatchBatchFromRoutedNoticesAsync_DispatchesExpectedContent()
  {
    string defaultStream = Guid
      .NewGuid()
      .ToString();
    EventStreamId defaultStreamId = (EventStreamId)defaultStream;
    CapturingNoticeIo noticeIo = new();
    TypedNoticeDispatcher ee = TypedNoticeDispatcher.Create(noticeIo);

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
    List<BatchRoutedTypedNoticeRequest> notices =
    [
      new(defaultStreamId, login1), new(defaultStreamId, login2),
      new(defaultStreamId, logout3), new(defaultStreamId, logout4)
    ];

    // Act
    BatchTypedNoticeDispatchResult result = await ee.DispatchBatchFromRoutedNoticesAsync(
      notices,
      obj => obj.GetHashCode().ToString(),
      options
    );

    OutputNotices(noticeIo.CapturedNotices);

    // Assert
    //   All the notices should have been returned as a success.
    notices.ShouldAllBe(notice =>
      result.Successes.Any(response => response.BatchNoticeId == notice.GetHashCode().ToString())
    );
    //   The dispatched notices should be returned.
    result.Successes.ShouldAllBe(response =>
      notices.Any(notice => ReferenceEquals(response.TypedNotice, notice.Notice))
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
      IoNoticeDispatchRequest actual = noticeIo.CapturedNotices.First(item =>
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
  public async Task DispatchBatchToSingleStreamAsync_DispatchesExpectedContent()
  {
    string defaultStream = Guid
      .NewGuid()
      .ToString();
    EventStreamId defaultStreamId = (EventStreamId)defaultStream;
    CapturingNoticeIo noticeIo = new();
    TypedNoticeDispatcher ee = TypedNoticeDispatcher.Create(noticeIo);

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
    List<object> notices =
    [
      login1, login2, logout3, logout4
    ];

    // Act
    BatchTypedNoticeDispatchResult result = await ee.DispatchBatchToSingleStreamAsync(
      defaultStreamId,
      notices,
      options: options,
      batchIdProvider: obj => obj.Notice.GetHashCode().ToString()
    );

    OutputNotices(noticeIo.CapturedNotices);

    // Assert
    //   All the notices should have been returned as a success.
    notices.ShouldAllBe(notice =>
      result.Successes.Any(response => response.BatchNoticeId == notice.GetHashCode().ToString())
    );
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
      IoNoticeDispatchRequest actual = noticeIo.CapturedNotices.First(item =>
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
