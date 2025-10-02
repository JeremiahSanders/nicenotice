using Microsoft.Extensions.DependencyInjection;

namespace Jds.NiceNotice;

public static class ServiceProviderExtensions
{
  public static ITypedNoticeDispatcher<TEnterpriseEventBaseType> GetEnterpriseEventDispatcher<
    TEnterpriseEventBaseType>(
    this IServiceProvider provider)
    where TEnterpriseEventBaseType : notnull
  {
    return MissingDependencyException.ThrowIfNull(provider.GetService<ITypedNoticeDispatcher<TEnterpriseEventBaseType>>());
  }
}
