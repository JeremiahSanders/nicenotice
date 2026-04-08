using Jds.NiceNotice.Dispatching.Implementations;
using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Custom;
using Jds.NiceNotice.TypedNotices;
using Jds.NiceNotice.TypedNotices.Routing;
using Jds.TestingUtils.Xunit2.Extras;

namespace Jds.NiceNotice.Tests.Unit.TypedNoticeDispatch.GenericDispatcher;

public abstract class GenericDispatcher_DirectConstruction_Fixture : BaseCaseFixture
{
  public readonly string defaultStream;
  public readonly EventStreamId defaultStreamId;
  public readonly TypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent> ee;
  public readonly CapturingNoticeIo noticeIo;

  protected GenericDispatcher_DirectConstruction_Fixture()
  {
    defaultStream = Guid
      .NewGuid()
      .ToString();
    defaultStreamId = (EventStreamId)defaultStream;
    noticeIo = new CapturingNoticeIo();
    ee =
      TypedNoticeDispatcher<ExampleCustomBaseEnterpriseEvent>.Create(
        noticeIo,
        Routers.Constant<ExampleCustomBaseEnterpriseEvent>(defaultStreamId)
      );
  }
}