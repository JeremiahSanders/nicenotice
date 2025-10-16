using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Jds.NiceNotice;

/// <summary>
///   A builder for configuring typed notices (enterprise events) which derive from a base type.
/// </summary>
/// <param name="services">The service collection to which services are added.</param>
/// <typeparam name="TEnterpriseEventBaseType">The base type for enterprise events.</typeparam>
public class TypedNoticesBuilder<TEnterpriseEventBaseType>(IServiceCollection services)
  where TEnterpriseEventBaseType : notnull
{
  /// <summary>
  ///   Gets the service collection (obtained from the constructor and to which services are added).
  /// </summary>
  public IServiceCollection Services => services;

  /// <summary>
  ///   Configures the <see cref="TypedNoticesBuilder{TEnterpriseEventBaseType}" /> instance with default services
  ///   and configurations necessary for operation.
  /// </summary>
  /// <remarks>
  ///   Registers a JSON <see cref="NoticeSerializer" />, a NoOp <see cref="NoticeValidator{TEnterpriseEventBaseType}" />,
  ///   and a <see cref="NoticeStreamSelector{TEnterpriseEventBaseType}" /> which always throws.
  /// </remarks>
  /// <param name="validatorServiceLifetime"></param>
  /// <param name="jsonSerializerServiceLifetime"></param>
  /// <param name="routerServiceLifetime"></param>
  /// <returns>
  ///   Returns the updated <see cref="TypedNoticesBuilder{TEnterpriseEventBaseType}" /> instance configured with
  ///   default settings.
  /// </returns>
  internal TypedNoticesBuilder<TEnterpriseEventBaseType> ApplyDefaults(
    ServiceLifetime jsonSerializerServiceLifetime = ServiceLifetime.Singleton,
    ServiceLifetime validatorServiceLifetime = ServiceLifetime.Singleton,
    ServiceLifetime routerServiceLifetime = ServiceLifetime.Singleton
  )
  {
    ApplyDefaults(Services, jsonSerializerServiceLifetime, validatorServiceLifetime, routerServiceLifetime);

    return this;
  }

  /// <summary>
  ///   Configures the <see cref="TypedNoticesBuilder{TEnterpriseEventBaseType}" /> instance with default services
  ///   and configurations necessary for operation.
  /// </summary>
  /// <remarks>
  ///   Registers a JSON <see cref="NoticeSerializer" />, a NoOp <see cref="NoticeValidator{TEnterpriseEventBaseType}" />,
  ///   and a <see cref="NoticeStreamSelector{TEnterpriseEventBaseType}" /> which always throws.
  /// </remarks>
  /// <param name="validatorServiceLifetime"></param>
  /// <param name="services"></param>
  /// <param name="jsonSerializerServiceLifetime"></param>
  /// <param name="routingServiceLifetime"></param>
  /// <returns>
  ///   Returns the updated <see cref="TypedNoticesBuilder{TEnterpriseEventBaseType}" /> instance configured with
  ///   default settings.
  /// </returns>
  internal static IServiceCollection ApplyDefaults(
    IServiceCollection services,
    ServiceLifetime jsonSerializerServiceLifetime = ServiceLifetime.Singleton,
    ServiceLifetime validatorServiceLifetime = ServiceLifetime.Singleton,
    ServiceLifetime routingServiceLifetime = ServiceLifetime.Singleton
  )
  {
    // Configure serializer
    ServiceDescriptor jsonSerializer =
      Serializers.CreateJsonSerializerDescriptor<TEnterpriseEventBaseType>(
        optionsAccessor: null,
        jsonSerializerServiceLifetime
      );
    services.TryAdd(jsonSerializer);
    ServiceDescriptor nonGeneric =
      Serializers.CreateNonGenericJsonSerializerDescriptor(optionsAccessor: null, jsonSerializerServiceLifetime);
    services.TryAdd(nonGeneric);

    // Configure validation
    ServiceDescriptor validator = new(
      typeof(NoticeValidator<TEnterpriseEventBaseType>),
      static _ => Validators.DataAnnotationsValidator<TEnterpriseEventBaseType>(),
      validatorServiceLifetime
    );
    services.TryAdd(validator);
    services.TryAdd(
      new ServiceDescriptor(
        typeof(NoticeValidator),
        static _ => new DataAnnotationsValidator(),
        validatorServiceLifetime
      )
    );

    // Configure routing
    ServiceDescriptor typeNameRouter =
      new(
        typeof(NoticeStreamSelector<TEnterpriseEventBaseType>),
        static _ => StreamSelectors.TypeNameStreams<TEnterpriseEventBaseType>(),
        routingServiceLifetime
      );
    services.TryAdd(typeNameRouter);

    return services;
  }

  #region Routing

  /// <summary>
  ///   Configures the algorithm used to identify the <see cref="EventStreamId" /> to which a notice should be dispatched.
  ///   Registers the provided instance in the service collection as a singleton.
  /// </summary>
  /// <remarks>
  ///   <para>This overload is most useful in test arrangement.</para>
  ///   <para>
  ///     It is expected that most runtime use cases will use the overload that uses a factory method, which provides
  ///     access to dependencies:
  ///     <see
  ///       cref="UseStreamSelector(Func{IServiceProvider, NoticeStreamSelector{TEnterpriseEventBaseType}}, ServiceLifetime)" />
  ///   </para>
  /// </remarks>
  /// <param name="streamSelector">
  ///   An implementation of <see cref="NoticeStreamSelector{TEnterpriseEventBaseType}" />.
  /// </param>
  /// <returns></returns>
  public TypedNoticesBuilder<TEnterpriseEventBaseType> UseStreamSelector(
    NoticeStreamSelector<TEnterpriseEventBaseType> streamSelector
  )
  {
    Services.Add(
      new ServiceDescriptor(typeof(NoticeStreamSelector<TEnterpriseEventBaseType>), streamSelector)
    );

    return this;
  }

  /// <summary>
  ///   Configures the algorithm used to identify the stream to which a notice should be dispatched
  ///   (determining its <see cref="EventStreamId" />).
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     See <see cref="StreamSelectors" /> for helper methods to create stream selectors.
  ///   </para>
  ///   <para>
  ///     Most projects find <see cref="StreamSelectors.TypeNameFactory" /> a good option,
  ///     sending every notice to a stream matching its type name.
  ///   </para>
  ///   <para>
  ///     For precise, type-based configuration, <see cref="StreamSelectors.TypeMap{TEnterpriseEventBaseType}" />
  ///     is a good fit.
  ///   </para>
  ///   <para>
  ///     In many custom cases, you can use the <see cref="StreamSelectors.Delegate" /> stream selector to provide custom
  ///     routing
  ///     logic. Use of <c>static</c> lambda methods is recommended.
  ///   </para>
  ///   <para>
  ///     Finally, <see cref="NoticeStreamSelector{TEnterpriseEventBaseType}" /> is abstract,
  ///     so you can create a custom implementation for the most control.
  ///   </para>
  /// </remarks>
  /// <param name="factory">
  ///   A factory method which receives an <see cref="IServiceProvider" />
  ///   and returns an implementation of <see cref="NoticeStreamSelector{TEnterpriseEventBaseType}" />.
  /// </param>
  /// <param name="lifetime">A service lifetime for the stream selector created by the <paramref name="factory" />.</param>
  /// <returns>Returns this instance for further configuration.</returns>
  public TypedNoticesBuilder<TEnterpriseEventBaseType> UseStreamSelector(
    Func<IServiceProvider, NoticeStreamSelector<TEnterpriseEventBaseType>> factory,
    ServiceLifetime lifetime
  )
  {
    Services.Add(
      new ServiceDescriptor(typeof(NoticeStreamSelector<TEnterpriseEventBaseType>), factory, lifetime)
    );

    return this;
  }

  #endregion

  #region Serialization

  /// <summary>
  ///   Configures the algorithm used to serialize enterprise events.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     The recommended implementation is <see cref="JsonNoticeSerializer{TEnterpriseEventBaseType}" />.
  ///     However, you can use any implementation of <see cref="NoticeSerializer{TEnterpriseEventBaseType}" />.
  ///   </para>
  /// </remarks>
  /// <param name="factory">
  ///   A factory method which receives an <see cref="IServiceProvider" /> and returns an implementation
  ///   of <see cref="NoticeSerializer{TEnterpriseEventBaseType}" />.
  /// </param>
  /// <param name="lifetime">The service lifetime of the instance returned by <paramref name="factory" />.</param>
  /// <returns>Returns this instance for further customization</returns>
  public TypedNoticesBuilder<TEnterpriseEventBaseType> UseSerializer(
    Func<IServiceProvider, NoticeSerializer<TEnterpriseEventBaseType>> factory,
    ServiceLifetime lifetime)
  {
    Services.Add(
      new ServiceDescriptor(typeof(NoticeSerializer<TEnterpriseEventBaseType>), factory, lifetime)
    );

    return this;
  }

  /// <summary>
  ///   Configures the algorithm used to serialize enterprise events.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     The recommended implementation is <see cref="JsonNoticeSerializer{TEnterpriseEventBaseType}" />.
  ///     However, you can use any implementation of <see cref="NoticeSerializer{TEnterpriseEventBaseType}" />.
  ///   </para>
  /// </remarks>
  /// <param name="serializer">
  ///   The <see cref="NoticeSerializer{TEnterpriseEventBaseType}" /> implementation to be used for serializing notices.
  /// </param>
  /// <returns>
  ///   Returns this instance for further customization.
  /// </returns>
  public TypedNoticesBuilder<TEnterpriseEventBaseType> UseSerializer(
    NoticeSerializer<TEnterpriseEventBaseType> serializer
  )
  {
    Services.Add(new ServiceDescriptor(typeof(NoticeSerializer<TEnterpriseEventBaseType>), serializer));

    return this;
  }

  #endregion

  #region Validation

  /// <summary>
  ///   Configures the enterprise event validation logic used, registering the provided instance as a singleton.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     See <see cref="Validators" /> for helper methods to create validators.
  ///   </para>
  ///   <para><see cref="Jds.NiceNotice.Validators.DataAnnotationsValidator" /> uses standard data annotation validation.</para>
  ///   <para>To skip validation, <see cref="Validators.NoOpValidator" />.</para>
  ///   <para>
  ///     For more complex or custom needs, try <see cref="DelegateNoticeValidator" />, or derive an implementation of
  ///     <see cref="NoticeValidator{TEnterpriseEventBaseType}" />.
  ///   </para>
  /// </remarks>
  /// <param name="factory">
  ///   A factory function which receives an <see cref="IServiceProvider" /> and returns an
  ///   implementation of <see cref="NoticeValidator{TEnterpriseEventBaseType}" />.
  /// </param>
  /// <param name="lifetime">A service lifetime for the validator created by the <paramref name="factory" />.</param>
  /// <returns>Returns this builder instance for further configuration.</returns>
  public TypedNoticesBuilder<TEnterpriseEventBaseType> UseValidator(
    Func<IServiceProvider, NoticeValidator<TEnterpriseEventBaseType>> factory,
    ServiceLifetime lifetime)
  {
    Services.Add(
      new ServiceDescriptor(typeof(NoticeValidator<TEnterpriseEventBaseType>), factory, lifetime)
    );

    return this;
  }

  /// <summary>
  ///   Configures the enterprise event validation logic used, registering the provided instance as a singleton.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     See <see cref="Validators" /> for helper methods to create validators.
  ///   </para>
  ///   <para><see cref="Jds.NiceNotice.Validators.DataAnnotationsValidator" /> uses standard data annotation validation.</para>
  ///   <para>To skip validation, <see cref="Validators.NoOpValidator" />.</para>
  ///   <para>
  ///     For more complex or custom needs, derive an implementation of
  ///     <see cref="NoticeValidator{TEnterpriseEventBaseType}" />.
  ///   </para>
  /// </remarks>
  /// <param name="validator">
  ///   An instance of <see cref="NoticeValidator{TEnterpriseEventBaseType}" /> to be used for validating events.
  /// </param>
  /// <returns>Returns this builder instance for further configuration.</returns>
  public TypedNoticesBuilder<TEnterpriseEventBaseType> UseValidator(
    NoticeValidator<TEnterpriseEventBaseType> validator)
  {
    Services.Add(
      new ServiceDescriptor(typeof(NoticeValidator<TEnterpriseEventBaseType>), validator)
    );

    return this;
  }

  #endregion
}
