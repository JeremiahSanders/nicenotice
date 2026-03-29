using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Standard;
using Jds.NiceNotice.TypedNotices;

namespace Jds.NiceNotice.Tests.Unit.TypedNoticeDispatch.NonGenericDispatcher.FromServiceProvider;

public class NonGenericDispatchAsyncFixture(bool actDispatchToFullNameStream)
  : GivenNiceNoticeServiceProviderWithoutTypedNoticeDeclaration
{
  public TypedNoticeDispatchResult<ExampleLogoutEnterpriseEvent> ActResponse { get; private set; } = null!;

  protected override async Task ActAsync()
  {
    await base.ActAsync();

    ActResponse =
      await ArrangedNonGenericTypedDispatcher.DispatchAsync(ArrangedMessage, actDispatchToFullNameStream);
  }
}
