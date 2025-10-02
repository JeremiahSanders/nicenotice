using Microsoft.Extensions.DependencyInjection;

namespace Jds.NiceNotice;

public static class ServiceCollectionExtensions
{
  /// <summary>
  ///   Adds cross-app notifications services to this service collection.
  /// </summary>
  /// <remarks>This is the primary entrypoint for adding cross-app notifications and typed &quot;enterprise events&quot;.</remarks>
  /// <param name="container">This service collection.</param>
  /// <param name="configure">A method which configures cross-app notifications.</param>
  /// <returns>Returns this service collection instance.</returns>
  public static IServiceCollection AddNiceNotice(
    this IServiceCollection container,
    Action<NiceNoticeBuilder> configure)
  {
    NiceNoticeBuilder builder = new(container);
    configure(builder);

    builder.ApplyDefaults();

    return container;
  }
}
