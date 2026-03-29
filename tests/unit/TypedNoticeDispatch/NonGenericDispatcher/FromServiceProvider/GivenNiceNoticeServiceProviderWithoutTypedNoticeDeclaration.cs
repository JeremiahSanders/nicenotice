using Jds.NiceNotice.Dispatching.Implementations;
using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Standard;
using Jds.TestingUtils.Randomization;
using Jds.TestingUtils.Xunit2.Extras;

using Microsoft.Extensions.DependencyInjection;

namespace Jds.NiceNotice.Tests.Unit.TypedNoticeDispatch.NonGenericDispatcher.FromServiceProvider;

/// <summary>
///   Tests verifying the behavior when <see cref="ServiceCollectionExtensions.AddNiceNotice" /> is invoked
///   and no typed notice configuration is provided.
///   This is the most basic configuration.
/// </summary>
public class GivenNiceNoticeServiceProviderWithoutTypedNoticeDeclaration : BaseCaseFixture
{
  /// <summary>
  ///   Tests verifying the behavior when <see cref="ServiceCollectionExtensions.AddNiceNotice" /> is invoked
  ///   and no typed notice configuration is provided.
  ///   This is the most basic configuration.
  /// </summary>
  public GivenNiceNoticeServiceProviderWithoutTypedNoticeDeclaration()
  {
    ArrangedDispatcher = new CapturingNoticeIo();
    ArrangedServiceProvider = new ServiceCollection()
      .AddNiceNotice(builder => builder.UseDispatcher(services => ArrangedDispatcher, ServiceLifetime.Singleton))
      .BuildServiceProvider();
    ArrangedNonGenericTypedDispatcher =
      ArrangedServiceProvider.GetRequiredService<ITypedNoticeDispatcher>();


    ArrangedMessage = new ExampleLogoutEnterpriseEvent
    {
      Username = Randomizer.Shared.RandomStringLatin(length: 24)
    };
  }

  public CapturingNoticeIo ArrangedDispatcher { get; }
  public ExampleLogoutEnterpriseEvent ArrangedMessage { get; }
  public ITypedNoticeDispatcher ArrangedNonGenericTypedDispatcher { get; }
  public ServiceProvider ArrangedServiceProvider { get; }
}
