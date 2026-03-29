using Xunit.Abstractions;

namespace Jds.NiceNotice.Tests.Unit.TypedNoticeDispatch;

internal static class OutputHelperExtensions
{
  public static void OutputNotices(this ITestOutputHelper outputHelper, IEnumerable<IoRequestNotice> capturedNotices)
  {
    outputHelper.WriteLine(message: "Captured notices:");
    foreach (IoRequestNotice notice in capturedNotices)
    {
      outputHelper.WriteLine($"{notice.Stream}: {notice.Notice}");
    }

    outputHelper.WriteLine(message: "----END NOTICES----");
  }
}
