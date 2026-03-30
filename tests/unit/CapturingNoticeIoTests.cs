using Jds.NiceNotice.Dispatching.Implementations;
using Jds.TestingUtils.Randomization;

using Shouldly;

namespace Jds.NiceNotice.Tests.Unit;

public class CapturingNoticeIoTests
{
  [Fact]
  public async Task AppliesCaptureLimit()
  {
    EventStreamId streamId = EventStreamId.From(value: "example-stream");
    const int toKeep = 3;
    const int toSend = 6;
    CapturingNoticeIo noticeIo = CapturingNoticeIo.Create(toKeep);

    for (int i = 0; i < toSend; i++)
    {
      IoNoticeDispatchResult _ = await noticeIo.DispatchAsync(IoNoticeDispatchRequest.Create(streamId, i.ToString()));
    }

    // Assert
    noticeIo
      .CapturedNotices.Count()
      .ShouldBe(toKeep);
    for (int i = 0; i < toKeep; i++)
    {
      noticeIo.CapturedNotices.ShouldContain(item =>
        item.Stream == streamId && item.Notice == (toSend - 1 - i).ToString()
      );
    }
  }

  [Fact]
  public async Task CapturesMessages()
  {
    EventStreamId streamId = EventStreamId.From(value: "example-stream");
    string notice =
      Randomizer.Shared.RandomStringLatin(Randomizer.Shared.IntInRange(minInclusive: 12, maxExclusive: 49));
    CapturingNoticeIo noticeIo = new();

    IoNoticeDispatchResult response = await noticeIo.DispatchAsync(IoNoticeDispatchRequest.Create(streamId, notice));

    response.Notice.ShouldBe(notice);
    noticeIo.CapturedNotices.ShouldContain(item => item.Stream == streamId && item.Notice == notice);
  }

  [Fact]
  public async Task Purge_ClearsMessages()
  {
    // Arrange
    EventStreamId streamId = EventStreamId.From(value: "example-stream");
    string notice =
      Randomizer.Shared.RandomStringLatin(Randomizer.Shared.IntInRange(minInclusive: 12, maxExclusive: 49));
    CapturingNoticeIo noticeIo = new();

    IoNoticeDispatchResult response = await noticeIo.DispatchAsync(IoNoticeDispatchRequest.Create(streamId, notice));

    // Sanity
    response.Notice.ShouldBe(notice);
    noticeIo.CapturedNotices.ShouldContain(item => item.Stream == streamId && item.Notice == notice);

    // Act
    noticeIo.PurgeNotices();

    // Assert
    noticeIo.CapturedNotices.ShouldBeEmpty();
  }
}
