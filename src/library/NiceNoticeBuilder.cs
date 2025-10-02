using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Jds.NiceNotice;

public class NiceNoticeBuilder(IServiceCollection services)
{
  public IServiceCollection Services => services;
  
  public NiceNoticeBuilder UseDispatcher<TDispatcher>(ServiceLifetime serviceLifetime)
    where TDispatcher : INoticeIo
  {
    services.Add(new ServiceDescriptor(typeof(INoticeIo), typeof(TDispatcher), serviceLifetime));

    return this;
  }

  public NiceNoticeBuilder UseDispatcher<TDispatcher>(
    Func<IServiceProvider, TDispatcher> resolver,
    ServiceLifetime serviceLifetime)
    where TDispatcher : INoticeIo
  {
    services.Add(new ServiceDescriptor(typeof(INoticeIo), provider => resolver(provider), serviceLifetime));

    return this;
  }

  /// <summary>
  ///   Adds support for dispatching typed, serialized notices (JSON most commonly).
  /// </summary>
  /// <param name="resolver">
  ///   A method which will provide the
  ///   <see cref="ITypedNoticeDispatcher{TEnterpriseEventBaseType}" /> service
  ///   when given an <see cref="IServiceProvider" />.
  /// </param>
  /// <param name="serviceLifetime">
  ///   A service lifetime to assign the
  ///   <see cref="ITypedNoticeDispatcher{TEnterpriseEventBaseType}" /> service.
  /// </param>
  /// <typeparam name="TNoticeBaseType">
  ///   A base notification type.
  ///   Use this to enforce an inheritance-based notice structure.
  /// </typeparam>
  /// <returns>Returns this instance.</returns>
  public NiceNoticeBuilder UseTypedNotices<TNoticeBaseType>(
    Func<IServiceProvider, ITypedNoticeDispatcher<TNoticeBaseType>> resolver,
    ServiceLifetime serviceLifetime)
    where TNoticeBaseType : notnull
  {
    services.Add(
      new ServiceDescriptor(typeof(ITypedNoticeDispatcher<TNoticeBaseType>), resolver, serviceLifetime)
    );

    return this;
  }

  /// <summary>
  ///   Adds support for dispatching typed, serialized notices (JSON most commonly).
  /// </summary>
  /// <param name="configure">A method which configures the handling of typed notices.</param>
  /// <param name="serviceLifetime">
  ///   A service lifetime to assign the
  ///   <see cref="ITypedNoticeDispatcher{TEnterpriseEventBaseType}" /> service.
  /// </param>
  /// <typeparam name="TNoticeBaseType">
  ///   A base notification type.
  ///   Use this to enforce an inheritance-based notice structure.
  /// </typeparam>
  /// <returns>Returns this instance.</returns>
  public NiceNoticeBuilder UseTypedNotices<TNoticeBaseType>(
    Action<TypedNoticesBuilder<TNoticeBaseType>> configure,
    ServiceLifetime serviceLifetime
  )
    where TNoticeBaseType : notnull
  {
    TypedNoticesBuilder<TNoticeBaseType> builder = new(services);

    configure(builder);

    builder.ApplyDefaults(serviceLifetime);

    services.TryAdd(
      new ServiceDescriptor(
        typeof(ITypedNoticeDispatcher<TNoticeBaseType>),
        typeof(DefaultTypedNoticeDispatcher<TNoticeBaseType>),
        serviceLifetime
      )
    );

    // By registering the non-generic typed notice dispatcher here we can apply a desired lifetime.
    TryAddNonGenericTypedNoticeDispatcher(serviceLifetime);

    return this;
  }

  private void TryAddNonGenericTypedNoticeDispatcher(ServiceLifetime? serviceLifetime = null)
  {
    services.TryAdd(
      new ServiceDescriptor(
        typeof(ITypedNoticeDispatcher),
        typeof(DefaultTypedNoticeDispatcher),
        serviceLifetime ?? ServiceLifetime.Transient
      )
    );
  }

  internal NiceNoticeBuilder ApplyDefaults()
  {
    // Try adding non-generic typed notice dispatch (in case it wasn't registered by UseTypedNotices.
    TryAddNonGenericTypedNoticeDispatcher();

    ServiceDescriptor dispatcherDescriptor = new(
      typeof(INoticeIo),
      typeof(NullNoticeIo),
      ServiceLifetime.Singleton
    );
    services.TryAdd(dispatcherDescriptor);

    return this;
  }
}
