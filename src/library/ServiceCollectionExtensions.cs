using Jds.NiceNotice.Configuration;

using Microsoft.Extensions.DependencyInjection;

namespace Jds.NiceNotice;

/// <summary>
///   Methods extending <see cref="IServiceCollection" /> to add cross-app notifications services.
/// </summary>
public static class ServiceCollectionExtensions
{
  /// <summary>
  ///   Adds NiceNotice cross-app notifications services to this service collection.
  /// </summary>
  /// <remarks>This is the primary entrypoint for adding cross-app notifications and typed &quot;enterprise events&quot;.</remarks>
  /// <param name="container">This service collection.</param>
  /// <param name="configure">A method which configures cross-app notifications.</param>
  /// <returns>Returns this service collection instance.</returns>
  public static IServiceCollection AddNiceNotice(
    this IServiceCollection container,
    Action<NiceNoticeBuilder> configure
  )
  {
    NiceNoticeBuilder builder = new(container);
    configure(builder);

    builder.ApplyDefaults();

    return container;
  }
}
