using Jds.TestingUtils.Xunit2.Extras;

using Xunit.Abstractions;

namespace Jds.NiceNotice.Tests.Unit.TypedNoticeDispatch;

public abstract class TypedNoticeDispatcherFixture(ITestOutputHelper outputHelper) : BaseCaseFixture
{
  protected void OutputNotices(IEnumerable<IoRequestNotice> capturedNotices)
  {
    outputHelper.OutputNotices(capturedNotices);
  }
}
