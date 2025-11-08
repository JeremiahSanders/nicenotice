using Jds.NiceNotice.Dispatching;
using Jds.NiceNotice.Dispatching.Implementations;
using Jds.NiceNotice.TypedNotices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Jds.NiceNotice.Configuration;

/// <summary>
///   A fluent builder for configuring NiceNotice services.
/// </summary>
/// <param name="services">A service collection to which NiceNotice is being added.</param>
public class NiceNoticeBuilder(IServiceCollection services)
{
  /// <summary>
  ///   Gets the service collection (obtained from the constructor and to which services are added).
  /// </summary>
  public IServiceCollection Services => services;

  /// <summary>
  ///   Registers a notice dispatcher (i.e. I/O).
  /// </summary>
  /// <param name="serviceLifetime">A service lifetime for the resolved instances.</param>
  /// <typeparam name="TDispatcher">A notice dispatch I/O type.</typeparam>
  /// <returns>Returns this builder instance.</returns>
  public NiceNoticeBuilder UseDispatcher<TDispatcher>(ServiceLifetime serviceLifetime)
    where TDispatcher : INoticeIo
  {
    // Try to add the dispatcher type directly (in case it hasn't been registered yet).
    Services.TryAdd(new ServiceDescriptor(typeof(TDispatcher), typeof(TDispatcher), serviceLifetime));

    // Then use a simple static resolver with the factory method overload.
    return UseDispatcher<TDispatcher>(
      static serviceProvider => serviceProvider.GetService<TDispatcher>() ??
                                throw MissingDependencyException.For<TDispatcher>(),
      serviceLifetime
    );
  }

  /// <summary>
  ///   Registers a notice dispatcher (i.e. I/O).
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     This overload is useful when your I/O implementation uses a static constructor or requires constructor
  ///     parameters which cannot be directly resolved from the application's service provider.
  ///   </para>
  /// </remarks>
  /// <param name="resolver">
  ///   A method which will provide the <see cref="INoticeIo" /> service when given an
  ///   <see cref="IServiceProvider" />.
  /// </param>
  /// <param name="serviceLifetime">A service lifetime for the resolved instances.</param>
  /// <typeparam name="TDispatcher">A notice dispatch I/O type.</typeparam>
  /// <returns>Returns this builder instance.</returns>
  public NiceNoticeBuilder UseDispatcher<TDispatcher>(
    Func<IServiceProvider, TDispatcher> resolver,
    ServiceLifetime serviceLifetime)
    where TDispatcher : INoticeIo
  {
    Services.Add(new ServiceDescriptor(typeof(INoticeIo), provider => resolver(provider), serviceLifetime));

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
  /// <param name="dispatcherServiceLifetime">
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
    ServiceLifetime dispatcherServiceLifetime)
    where TNoticeBaseType : notnull
  {
    Services.Add(
      new ServiceDescriptor(typeof(ITypedNoticeDispatcher<TNoticeBaseType>), resolver, dispatcherServiceLifetime)
    );

    return this;
  }

  /// <summary>
  ///   Adds support for dispatching typed, serialized notices (JSON most commonly)
  ///   using the default <see cref="EnterpriseEvent" /> as the assumed base type.
  /// </summary>
  /// <remarks>
  ///   Use the
  ///   <see
  ///     cref="UseTypedNotices{TNoticeBaseType}(System.Action{Jds.NiceNotice.Configuration.TypedNoticesBuilder{TNoticeBaseType}},Microsoft.Extensions.DependencyInjection.ServiceLifetime)" />
  ///   overload to specify a different base type.
  /// </remarks>
  /// <param name="configure">A method which configures the handling of typed notices.</param>
  /// <param name="dispatcherServiceLifetime">
  ///   A service lifetime to assign the
  ///   <see cref="ITypedNoticeDispatcher{TEnterpriseEventBaseType}" /> service.
  ///   The typed notice dispatcher depends upon the configured <see cref="INoticeIo" />
  ///   (such as the adapters provided by <c>NiceNotice.Aws.Sns</c> NuGet package).
  ///   Be considerate of the thread-safety and best practices of your I/O implementation.
  /// </param>
  /// <returns>Returns this instance.</returns>
  public NiceNoticeBuilder UseTypedNotices(
    Action<TypedNoticesBuilder<EnterpriseEvent>> configure,
    ServiceLifetime dispatcherServiceLifetime
  )
  {
    return UseTypedNotices<EnterpriseEvent>(configure, dispatcherServiceLifetime);
  }

  /// <summary>
  ///   Adds support for dispatching typed, serialized notices (JSON most commonly).
  /// </summary>
  /// <param name="configure">A method which configures the handling of typed notices.</param>
  /// <param name="dispatcherServiceLifetime">
  ///   <para>
  ///     A service lifetime to assign the
  ///     <see cref="ITypedNoticeDispatcher{TEnterpriseEventBaseType}" />
  ///     and <see cref="ITypedNoticeDispatcher" /> services.
  ///   </para>
  ///   <para>
  ///     Advice: Use <see cref="ServiceLifetime.Transient" /> for console applications
  ///     and <see cref="ServiceLifetime.Scoped" /> for web applications.
  ///   </para>
  /// </param>
  /// <typeparam name="TNoticeBaseType">
  ///   A base notification type.
  ///   Use this to enforce an inheritance-based notice structure.
  /// </typeparam>
  /// <returns>Returns this instance.</returns>
  public NiceNoticeBuilder UseTypedNotices<TNoticeBaseType>(
    Action<TypedNoticesBuilder<TNoticeBaseType>> configure,
    ServiceLifetime dispatcherServiceLifetime
  )
    where TNoticeBaseType : notnull
  {
    TypedNoticesBuilder<TNoticeBaseType> builder = new(services);

    configure(builder);

    builder.ApplyDefaults();

    TryAddGenericTypedNoticeDispatcher<TNoticeBaseType>(dispatcherServiceLifetime);

    // By registering the non-generic typed notice dispatcher here we can apply a desired lifetime.
    TryAddNonGenericTypedNoticeDispatcher(dispatcherServiceLifetime);

    return this;
  }

  private void TryAddGenericTypedNoticeDispatcher<TNoticeBaseType>(ServiceLifetime dispatcherServiceLifetime)
    where TNoticeBaseType : notnull
  {
    Services.TryAdd(
      new ServiceDescriptor(
        typeof(ITypedNoticeDispatcher<TNoticeBaseType>),
        typeof(DefaultTypedNoticeDispatcher<TNoticeBaseType>),
        dispatcherServiceLifetime
      )
    );
  }

  private void TryAddNonGenericTypedNoticeDispatcher(ServiceLifetime? serviceLifetime = null)
  {
    Services.TryAdd(
      new ServiceDescriptor(
        typeof(ITypedNoticeDispatcher),
        typeof(DefaultTypedNoticeDispatcher),
        serviceLifetime ?? ServiceLifetime.Transient
      )
    );
  }

  internal NiceNoticeBuilder ApplyDefaults()
  {
    // UseTypedNotices should use TryAdd for its configuration, so this shouldn't override anything the user configured.
    UseTypedNotices<EnterpriseEvent>(static _ => { }, ServiceLifetime.Transient);

    ServiceDescriptor dispatcherDescriptor = new(
      typeof(INoticeIo),
      typeof(NullNoticeIo),
      ServiceLifetime.Singleton // NullNoticeIo is thread-safe.
    );
    Services.TryAdd(dispatcherDescriptor);

    return this;
  }
}
