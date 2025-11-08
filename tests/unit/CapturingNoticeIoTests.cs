using Jds.NiceNotice.Dispatching.Implementations;
using Jds.TestingUtils.Randomization;

using Shouldly;

namespace Jds.NiceNotice.Tests.Unit;

public class CapturingNoticeIoTests
{
  [Fact]
  public async Task CapturesMessages()
  {
    EventStreamId streamId = EventStreamId.From(value: "example-stream");
    string notice =
      Randomizer.Shared.RandomStringLatin(Randomizer.Shared.IntInRange(minInclusive: 12, maxExclusive: 49));
    CapturingNoticeIo noticeIo = new();

    string response = await noticeIo.DispatchAsync(streamId, notice);

    response.ShouldBe(notice);
    noticeIo.CapturedNotices.ShouldContain(item => item.Item1 == streamId && item.Item2 == notice);
  }

  [Fact]
  public async Task AppliesCaptureLimit()
  {
    EventStreamId streamId = EventStreamId.From(value: "example-stream");
    const int toKeep = 3;
    const int toSend = 6;
    CapturingNoticeIo noticeIo = CapturingNoticeIo.Create(toKeep);

    for (int i = 0; i < toSend; i++)
    {
      string _ = await noticeIo.DispatchAsync(streamId, i.ToString());
    }

    // Assert
    noticeIo
      .CapturedNotices.Count()
      .ShouldBe(toKeep);
    for (int i = 0; i < toKeep; i++)
    {
      noticeIo.CapturedNotices.ShouldContain(item => item.Item1 == streamId && item.Item2 == (toSend - 1 - i).ToString()
      );
    }
  }

  [Fact]
  public async Task Purge_ClearsMessages()
  {
    // Arrange
    EventStreamId streamId = EventStreamId.From(value: "example-stream");
    string notice =
      Randomizer.Shared.RandomStringLatin(Randomizer.Shared.IntInRange(minInclusive: 12, maxExclusive: 49));
    CapturingNoticeIo noticeIo = new();

    string response = await noticeIo.DispatchAsync(streamId, notice);

    // Sanity
    response.ShouldBe(notice);
    noticeIo.CapturedNotices.ShouldContain(item => item.Item1 == streamId && item.Item2 == notice);

    // Act
    noticeIo.PurgeNotices();

    // Assert
    noticeIo.CapturedNotices.ShouldBeEmpty();
  }
}
