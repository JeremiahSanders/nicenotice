using System.Net.Mime;

using Jds.NiceNotice.Dispatching;
using Jds.NiceNotice.Dispatching.Implementations;
using Jds.NiceNotice.TypedNotices;
using Jds.NiceNotice.TypedNotices.Serialization;
using Jds.NiceNotice.TypedNotices.Validation;
using Jds.TestingUtils.Randomization;

using Shouldly;

namespace Jds.NiceNotice.Tests.Unit;

public class BatchDispatchingWorkflowTests
{
  private CapturingNoticeIo CapturingIo { get; } = CapturingNoticeIo.Create();
  private NoticeSerializer JsonSerializer { get; } = Serializers.Json();
  private NoticeValidator NoticeValidator { get; } = Validators.DataAnnotationsValidator();

  [Fact]
  public async Task CatchesBatchDispatchingExceptions()
  {
    INoticeIo noticeIo = DelegateNoticeIo.AlwaysFails();
    string batchItemId = Guid.NewGuid().ToString();
    EventStreamId eventStream = EventStreamId.From(Randomizer.Shared.RandomStringLatin(length: 9));
    EnterpriseEvent notice = new()
    {
      Schema = "my-custom-event"
    };
    Dictionary<string, BatchRoutedTypedNoticeRequest> notices = new()
    {
      {
        batchItemId, new BatchRoutedTypedNoticeRequest(
          eventStream,
          notice
        )
      }
    };

    Either<Exception, BatchTypedNoticeDispatchResult> result = await ActAsync(notices, noticeIo: noticeIo);

    result.IsRight.ShouldBeTrue();
    BatchTypedNoticeDispatchResult actual = result.RightUnsafe.ShouldNotBeNull();
    actual.Failures.ShouldContain(tuple =>
      ReferenceEquals(tuple.TypedNotice, notice) && tuple.Stream == eventStream
    );
  }

  internal async Task<Either<Exception, BatchTypedNoticeDispatchResult>> ActAsync(
    IReadOnlyDictionary<string, BatchRoutedTypedNoticeRequest> notices,
    BatchDispatchOptions? batchDispatchOptions = null,
    INoticeIo? noticeIo = null)
  {
    Either<Exception, BatchTypedNoticeDispatchResult> result = await Eithers.TryAsync(() =>
      BatchDispatchingWorkflow.DispatchBatchAsync(
        () => noticeIo ?? CapturingIo,
        TrySerializeAndValidate,
        notices,
        batchDispatchOptions
      )
    );

    return result;

    BatchRoutedTypedNoticeResponse TrySerializeAndValidate(
      string arg1,
      BatchRoutedTypedNoticeRequest arg2)
    {
      string serialized = JsonSerializer.Serialize(arg2.Notice);
      IReadOnlyList<string>? validationResult = NoticeValidator.Validate(arg2.Notice, serialized);

      Exception? exception = validationResult?.Count > 0 ? new Exception(message: "Validation failed") : null;
      BatchRoutedTypedNoticeResponse response = new(
        arg1,
        arg2.Stream,
        arg2.Notice,
        serialized,
        MediaTypeNames.Application.Json,
        metadata: null,
        exception
      );

      return response;
    }
  }
}
