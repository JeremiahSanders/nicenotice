using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Jds.NiceNotice;

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
  /// <returns>
  ///   Returns the updated <see cref="TypedNoticesBuilder{TEnterpriseEventBaseType}" /> instance configured with
  ///   default settings.
  /// </returns>
  internal TypedNoticesBuilder<TEnterpriseEventBaseType> ApplyDefaults(
    ServiceLifetime jsonSerializerServiceLifetime = ServiceLifetime.Singleton,
    ServiceLifetime validatorServiceLifetime = ServiceLifetime.Singleton
  )
  {
    ApplyDefaults(Services, jsonSerializerServiceLifetime, validatorServiceLifetime);

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
      CreateJsonSerializerDescriptor(optionsAccessor: null, jsonSerializerServiceLifetime);
    services.TryAdd(jsonSerializer);
    ServiceDescriptor nonGeneric =
      CreateNonGenericJsonSerializerDescriptor(optionsAccessor: null, jsonSerializerServiceLifetime);
    services.TryAdd(nonGeneric);

    // Configure validation
    ServiceDescriptor validator = new(
      typeof(NoticeValidator<TEnterpriseEventBaseType>),
      static _ => new NoOpNoticeValidator<TEnterpriseEventBaseType>(),
      validatorServiceLifetime
    );
    services.TryAdd(validator);

    // Configure routing
    ServiceDescriptor notifyOfOmittedConfigurationStreamSelector =
      new(
        typeof(NoticeStreamSelector<TEnterpriseEventBaseType>),
        static _ => new InvalidOperationStreamSelector<TEnterpriseEventBaseType>(),
        routingServiceLifetime
      );
    services.TryAdd(notifyOfOmittedConfigurationStreamSelector);

    return services;
  }


  #region Routing

  public TypedNoticesBuilder<TEnterpriseEventBaseType> UseStreamSelector(
    NoticeStreamSelector<TEnterpriseEventBaseType> streamSelector
  )
  {
    Services.Add(
      new ServiceDescriptor(typeof(NoticeStreamSelector<TEnterpriseEventBaseType>), streamSelector)
    );

    return this;
  }

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

  public TypedNoticesBuilder<TEnterpriseEventBaseType> UseSerializer(
    Func<IServiceProvider, NoticeSerializer<TEnterpriseEventBaseType>> factory,
    ServiceLifetime lifetime)
  {
    Services.Add(
      new ServiceDescriptor(typeof(NoticeSerializer<TEnterpriseEventBaseType>), factory, lifetime)
    );

    return this;
  }

  public TypedNoticesBuilder<TEnterpriseEventBaseType> UseSerializer(
    NoticeSerializer<TEnterpriseEventBaseType> serializer)
  {
    Services.Add(new ServiceDescriptor(typeof(NoticeSerializer<TEnterpriseEventBaseType>), serializer));

    return this;
  }

  /// <summary>
  ///   Configures the enterprise event builder to use a JSON serializer for serializing events.
  /// </summary>
  /// <param name="optionsAccessor">
  ///   A function to provide <see cref="JsonSerializerOptions" /> at runtime, or null to use default options
  ///   (<see cref="JsonDefaults.DefaultJsonSerializerOptions" />).
  /// </param>
  /// <param name="serviceLifetime">
  ///   The lifetime of the serializer service. Defaults to <see cref="ServiceLifetime.Singleton" />.
  /// </param>
  /// <returns>
  ///   Returns the modified <see cref="TypedNoticesBuilder{TEnterpriseEventBaseType}" /> instance configured to use the
  ///   JSON serializer.
  /// </returns>
  public TypedNoticesBuilder<TEnterpriseEventBaseType> WithJsonSerializer(
    Func<IServiceProvider, JsonSerializerOptions>? optionsAccessor = null,
    ServiceLifetime serviceLifetime = ServiceLifetime.Singleton
  )
  {
    ServiceDescriptor descriptor = CreateJsonSerializerDescriptor(optionsAccessor, serviceLifetime);
    Services.Add(descriptor);
    ServiceDescriptor nonGeneric = CreateNonGenericJsonSerializerDescriptor(optionsAccessor, serviceLifetime);
    Services.Add(nonGeneric);

    return this;
  }

  private static ServiceDescriptor CreateJsonSerializerDescriptor(
    Func<IServiceProvider, JsonSerializerOptions>? optionsAccessor,
    ServiceLifetime serviceLifetime)
  {
    return new ServiceDescriptor(
      typeof(NoticeSerializer<TEnterpriseEventBaseType>),
      optionsAccessor != null ? WithOptionsAccessor : WithConfiguredOptions,
      serviceLifetime
    );

    static NoticeSerializer<TEnterpriseEventBaseType> WithConfiguredOptions(IServiceProvider runtimeServiceProvider)
    {
      JsonSerializerOptions? possibleOptions = runtimeServiceProvider.GetService<JsonSerializerOptions>();
      JsonNoticeSerializer<TEnterpriseEventBaseType> func = new(possibleOptions);

      return func;
    }

    NoticeSerializer<TEnterpriseEventBaseType> WithOptionsAccessor(IServiceProvider runtimeServiceProvider)
    {
      JsonSerializerOptions? possibleOptions = optionsAccessor.Invoke(runtimeServiceProvider);
      JsonNoticeSerializer<TEnterpriseEventBaseType> func = new(possibleOptions);

      return func;
    }
  }

  private static ServiceDescriptor CreateNonGenericJsonSerializerDescriptor(
    Func<IServiceProvider, JsonSerializerOptions>? optionsAccessor,
    ServiceLifetime serviceLifetime)
  {
    return new ServiceDescriptor(
      typeof(NoticeSerializer),
      object (runtimeServiceProvider) =>
      {
        JsonSerializerOptions? possibleOptions = optionsAccessor?.Invoke(runtimeServiceProvider) ??
                                                 runtimeServiceProvider.GetService<JsonSerializerOptions>();
        JsonNoticeSerializer func = new(possibleOptions);

        return func;
      },
      serviceLifetime
    );
  }

  #endregion

  #region Validation

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
  ///   Configures the enterprise event builder to use a validator for validating events.
  /// </summary>
  /// <param name="validator">
  ///   An instance of <see cref="NoticeValidator{TEnterpriseEventBaseType}" /> to be used for validating events.
  /// </param>
  /// <returns>
  ///   Returns the modified <see cref="TypedNoticesBuilder{TEnterpriseEventBaseType}" /> instance configured to use the
  ///   provided validator.
  /// </returns>
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
