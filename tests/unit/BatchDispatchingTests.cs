using Jds.TestingUtils.Randomization;

using Shouldly;

namespace Jds.NiceNotice.Tests.Unit;

public class BatchDispatchingTests
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
    Dictionary<string, BatchedRoutedTypedNotice> notices = new()
    {
      {
        batchItemId, new BatchedRoutedTypedNotice(
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
    IReadOnlyDictionary<string, BatchedRoutedTypedNotice> notices,
    BatchDispatchOptions? batchDispatchOptions = null,
    INoticeIo? noticeIo = null)
  {
    Either<Exception, BatchTypedNoticeDispatchResult> result = await Eithers.TryAsync(() =>
      BatchDispatching.DispatchBatchAsync(
        noticeIo ?? CapturingIo,
        TrySerializeAndValidate,
        notices,
        batchDispatchOptions
      )
    );

    return result;

    Either<(BatchRoutedTypedNoticeResponse, Exception), BatchRoutedTypedNoticeResponse> TrySerializeAndValidate(
      string arg1,
      BatchedRoutedTypedNotice arg2)
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
