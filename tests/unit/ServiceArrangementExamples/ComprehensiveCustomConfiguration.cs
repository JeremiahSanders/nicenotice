using Jds.NiceNotice.Configuration;
using Jds.NiceNotice.Tests.Unit.ExampleApplication;
using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Custom;

using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace Jds.NiceNotice.Tests.Unit.ServiceArrangementExamples;

public static class ComprehensiveCustomConfiguration
{
  /// <summary>
  ///   <para>
  ///     Test arrangement:
  ///     Uses <see cref="ExampleCustomBaseEnterpriseEvent" /> as the base enterprise event type.
  ///     All three typed notice services are explicitly registered with custom implementations.
  ///     We register a custom dispatcher which sends notices to xUnit's test output.
  ///   </para>
  /// </summary>
  public static IServiceCollection ApplyComprehensiveCustomConfiguration(
    this IServiceCollection services,
    ITestOutputHelper testOutputHelper
  )
  {
    return services.AddNiceNotice(builder => builder
      .UseTypedNotices<ExampleCustomBaseEnterpriseEvent>(
        eeBuilder => eeBuilder
          .UseSerializer(static serviceProvider => new ExampleCustomNoticeSerializer(), ServiceLifetime.Singleton)
          .UseStreamSelector(static serviceProvider => new ExampleCustomRouter(), ServiceLifetime.Singleton)
          .UseValidator(
            static serviceProvider => new ExampleCustomNoticeValidator(maxNoticeSize: 2048),
            ServiceLifetime.Singleton
          ),
        ServiceLifetime.Transient
      )
      .UseDispatcher(serviceProvider => new ExampleCustomDispatcher(testOutputHelper), ServiceLifetime.Singleton)
    );
  }
}
