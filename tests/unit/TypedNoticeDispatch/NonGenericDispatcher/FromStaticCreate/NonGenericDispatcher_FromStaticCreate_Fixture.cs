using Jds.NiceNotice.Dispatching.Implementations;
using Jds.NiceNotice.TypedNotices;
using Jds.TestingUtils.Xunit2.Extras;

namespace Jds.NiceNotice.Tests.Unit.TypedNoticeDispatch.NonGenericDispatcher.FromStaticCreate;

public abstract class NonGenericDispatcher_FromStaticCreate_Fixture : BaseCaseFixture
{
  protected NonGenericDispatcher_FromStaticCreate_Fixture()
  {
    ArrangedDefaultStream = Guid
      .NewGuid()
      .ToString();
    ArrangedDefaultStreamId = (EventStreamId)ArrangedDefaultStream;
    ArrangedNoticeIo = new CapturingNoticeIo();
    ArrangedTypedNoticeDispatcher = TypedNoticeDispatcher.Create(ArrangedNoticeIo);
  }

  public string ArrangedDefaultStream { get; set; }

  public EventStreamId ArrangedDefaultStreamId { get; set; }

  public CapturingNoticeIo ArrangedNoticeIo { get; set; }
  public ITypedNoticeDispatcher ArrangedTypedNoticeDispatcher { get; set; }
}