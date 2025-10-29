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
  private void OutputNotices(IEnumerable<(EventStreamId, string)> capturedNotices)
  {
    outputHelper.WriteLine(message: "Captured notices:");
    foreach ((EventStreamId, string) notice in capturedNotices)
    {
      outputHelper.WriteLine($"{notice.Item1}: {notice.Item2}");
    }

    outputHelper.WriteLine(message: "----END NOTICES----");
  }

  /// <summary>
  ///   Tests verifying use of <see cref="Jds.NiceNotice.NiceNoticeBuilder.UseTypedNotices" /> without any
  ///   type arguments.
  ///   This is the second-most basic configuration.
  ///   In this arrangement, we will assume that <see cref="EnterpriseEvent" /> is the default base type.
  /// </summary>
  /// <param name="testOutputHelper"></param>
  public class DefaultTypedEvents(ITestOutputHelper testOutputHelper) : TypedNoticeDispatcherTests(testOutputHelper)
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
  }

  /// <summary>
  ///   Tests verifying the behavior when <see cref="Jds.NiceNotice.ServiceCollectionExtensions.AddNiceNotice" /> is invoked
  ///   and no typed notice configuration is provided.
  ///   This is the most basic configuration.
  /// </summary>
  /// <param name="testOutputHelper"></param>
  public class DefaultInitializer(ITestOutputHelper testOutputHelper) : TypedNoticeDispatcherTests(testOutputHelper)
  {
    [Fact]
    public async Task NonGenericDispatchAsync_DispatchesExpectedContent()
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
    public async Task NonGenericDispatchAsync_WithFullName_DispatchesExpectedContent()
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
  }

  /// <summary>
  ///   This tests the behavior of a <see cref="Jds.NiceNotice.TypedNoticeDispatcher" /> (the non-generic version)
  ///   in isolation (i.e., not related to the <see cref="Jds.NiceNotice.ServiceCollectionExtensions.AddNiceNotice" />
  ///   extension method).
  /// </summary>
  /// <param name="testOutputHelper"></param>
  public class NonGenericDispatcher(ITestOutputHelper testOutputHelper) : TypedNoticeDispatcherTests(testOutputHelper)
  {
    [Fact]
    public async Task DispatchAsync_DispatchesExpectedContent()
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
    public async Task DispatchBatchAsync_CapturesBatchIoFailures()
    {
      string defaultStream = Guid
        .NewGuid()
        .ToString();
      EventStreamId defaultStreamId = (EventStreamId)defaultStream;
      INoticeBatchIo noticeIo = DelegateBatchNoticeIo.AlwaysFails();
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

      DispatchBatchRequest request = DispatchBatchRequest.Create(
        defaultStreamId,
        [login1, login2],
        new BatchDispatchOptions
        {
          MaxDegreeOfParallelism = 4
        }
      );

      // Act
      BatchTypedNoticeDispatchResult response = await ee.DispatchBatchAsync(request);

      // Assert
      request.Notices.ShouldAllBe(kvp => response.Failures.Any(tuple => tuple.Item1.BatchNoticeId == kvp.Key));
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

      DispatchBatchRequest request = DispatchBatchRequest.Create(
        defaultStreamId,
        [login1, login2],
        new BatchDispatchOptions
        {
          MaxDegreeOfParallelism = 4
        }
      );

      // Act
      BatchTypedNoticeDispatchResult response = await ee.DispatchBatchAsync(request);

      // Assert
      request.Notices.ShouldAllBe(kvp => response.Failures.Any(tuple => tuple.Item1.BatchNoticeId == kvp.Key));
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
      DispatchBatchRequest request = DispatchBatchRequest.Create(
        defaultStreamId,
        [
          login1, login2, logout3, logout4
        ],
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
        request.Notices[response.BatchNoticeId].Notice == response.Notice
      );

      AssertNoticeWasDispatched(login1.Username, login1);
      AssertNoticeWasDispatched(login2.Username, login2);
      AssertNoticeWasDispatched(logout3.Username, logout3);
      AssertNoticeWasDispatched(logout4.Username, logout4);

      return;

      void AssertNoticeWasDispatched(string valueToFind, object expected)
      {
        // The notice should have gone to the default stream and it should contain the value to find.
        noticeIo.CapturedNotices.ShouldContain(item =>
          item.Item1 == (EventStreamId)defaultStream &&
          item.Item2.Contains(valueToFind)
        );

        // The notice should have been serialized to JSON as expected.
        (EventStreamId _, string actual) = noticeIo.CapturedNotices.First(item =>
          item.Item1 == (EventStreamId)defaultStream &&
          item.Item2.Contains(valueToFind)
        );
        string expectedJson = JsonSerializer.Serialize(expected, JsonDefaults.DefaultJsonSerializerOptions);

        actual.ShouldBe(expectedJson);
      }
    }
  }

  /// <summary>
  ///   This tests the behavior of a <see cref="Jds.NiceNotice.TypedNoticeDispatcher{TEnterpriseEventBaseType}" />
  ///   (the generic version) in isolation
  ///   (i.e., not related to the <see cref="Jds.NiceNotice.ServiceCollectionExtensions.AddNiceNotice" />
  ///   extension method).
  /// </summary>
  /// <param name="testOutputHelper"></param>
  public class GenericDispatcher(ITestOutputHelper testOutputHelper) : TypedNoticeDispatcherTests(testOutputHelper)
  {
    [Fact]
    public async Task DispatchAsync_DispatchesExpectedContent()
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

    [Fact]
    public async Task DispatchBatchAsync_CapturesBatchIoFailures()
    {
      string defaultStream = Guid
        .NewGuid()
        .ToString();
      EventStreamId defaultStreamId = (EventStreamId)defaultStream;
      INoticeBatchIo noticeIo = DelegateBatchNoticeIo.AlwaysFails();
      TypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent> ee =
        TypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent>.Create(
          noticeIo,
          StreamSelectors.Constant<ExampleCustomBaseEnterpriseEvent>(defaultStreamId)
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
        DispatchBatchRequest<ExampleCustomBaseEnterpriseEvent>.Create(
          [login1, login2],
          new BatchDispatchOptions
          {
            MaxDegreeOfParallelism = 4
          }
        );

      // Act
      BatchTypedNoticeDispatchResult response = await ee.DispatchBatchAsync(request);

      // Assert
      request.Notices.ShouldAllBe(kvp => response.Failures.Any(tuple => tuple.Item1.BatchNoticeId == kvp.Key));
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
          StreamSelectors.Constant<ExampleCustomBaseEnterpriseEvent>(defaultStreamId)
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
        DispatchBatchRequest<ExampleCustomBaseEnterpriseEvent>.Create(
          [login1, login2],
          new BatchDispatchOptions
          {
            MaxDegreeOfParallelism = 4
          }
        );

      // Act
      BatchTypedNoticeDispatchResult response = await ee.DispatchBatchAsync(request);

      // Assert
      request.Notices.ShouldAllBe(kvp => response.Failures.Any(tuple => tuple.Item1.BatchNoticeId == kvp.Key));
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
          StreamSelectors.Constant<ExampleCustomBaseEnterpriseEvent>(defaultStreamId)
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
        DispatchBatchRequest<ExampleCustomBaseEnterpriseEvent>.Create(
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
        request.Notices[response.BatchNoticeId] == (ExampleCustomBaseEnterpriseEvent)response.Notice
      );

      AssertNoticeWasDispatched(login1.Username, login1);
      AssertNoticeWasDispatched(login2.Username, login2);
      AssertNoticeWasDispatched(logout3.Username, logout3);
      AssertNoticeWasDispatched(logout4.Username, logout4);

      return;

      void AssertNoticeWasDispatched(string valueToFind, object expected)
      {
        // The notice should have gone to the default stream and it should contain the value to find.
        noticeIo.CapturedNotices.ShouldContain(item =>
          item.Item1 == (EventStreamId)defaultStream &&
          item.Item2.Contains(valueToFind)
        );

        // The notice should have been serialized to JSON as expected.
        (EventStreamId _, string actual) = noticeIo.CapturedNotices.First(item =>
          item.Item1 == (EventStreamId)defaultStream &&
          item.Item2.Contains(valueToFind)
        );
        string expectedJson = JsonSerializer.Serialize(expected, JsonDefaults.DefaultJsonSerializerOptions);

        actual.ShouldBe(expectedJson);
      }
    }
  }
}
