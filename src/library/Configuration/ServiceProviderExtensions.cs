using Jds.NiceNotice.TypedNotices;

using Microsoft.Extensions.DependencyInjection;

namespace Jds.NiceNotice.Configuration;

/// <summary>
///   Methods extending <see cref="IServiceProvider" /> to support cross-app notifications services.
/// </summary>
public static class ServiceProviderExtensions
{
  /// <summary>
  ///   Retrieves an instance of <see cref="ITypedNoticeDispatcher{TEnterpriseEventBaseType}" /> from the specified
  ///   <see cref="IServiceProvider" />.
  /// </summary>
  /// <typeparam name="TEnterpriseEventBaseType">
  ///   The base type of the enterprise event for which the dispatcher operates.
  ///   Must be a non-nullable type.
  /// </typeparam>
  /// <param name="provider">The <see cref="IServiceProvider" /> instance from which to resolve the dispatcher.</param>
  /// <returns>
  ///   An instance of <see cref="ITypedNoticeDispatcher{TEnterpriseEventBaseType}" /> if registered; otherwise,
  ///   throws a <see cref="MissingDependencyException" />.
  /// </returns>
  public static ITypedNoticeDispatcher<TEnterpriseEventBaseType> GetEnterpriseEventDispatcher<TEnterpriseEventBaseType>(
    this IServiceProvider provider
  )
    where TEnterpriseEventBaseType : notnull
  {
    return MissingDependencyException.ThrowIfNull(
      provider.GetService<ITypedNoticeDispatcher<TEnterpriseEventBaseType>>()
    );
  }
}
