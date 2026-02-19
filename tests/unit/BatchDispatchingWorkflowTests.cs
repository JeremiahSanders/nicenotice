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
      ReferenceEquals(tuple.Item1.Notice, notice) && tuple.Item1.Stream == eventStream
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

    Either<(BatchRoutedTypedNoticeResponse, Exception), BatchRoutedTypedNoticeResponse> TrySerializeAndValidate(
      string arg1,
      BatchRoutedTypedNoticeRequest arg2)
    {
      string serialized = JsonSerializer.Serialize(arg2.Notice);
      IReadOnlyList<string>? validationResult = NoticeValidator.Validate(arg2.Notice, serialized);

      BatchRoutedTypedNoticeResponse response = new(arg1, arg2.Stream, arg2.Notice, serialized);
      if (validationResult?.Count > 0)
      {
        return Eithers.Left<(BatchRoutedTypedNoticeResponse, Exception), BatchRoutedTypedNoticeResponse>(
          (response, new Exception(message: "Validation failed"))
        );
      }

      return Eithers.Right<(BatchRoutedTypedNoticeResponse, Exception), BatchRoutedTypedNoticeResponse>(response);
    }
  }
}
