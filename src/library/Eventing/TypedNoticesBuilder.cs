using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Jds.NiceNotice;

public class TypedNoticesBuilder<TEnterpriseEventBaseType>(IServiceCollection services)
  where TEnterpriseEventBaseType : notnull
{
  internal TypedNoticesBuilder<TEnterpriseEventBaseType> ApplyDefaults(ServiceLifetime serviceLifetime)
  {
    ServiceDescriptor jsonSerializer = CreateJsonSerializerDescriptor(optionsAccessor: null, serviceLifetime);
    services.TryAdd(jsonSerializer);
    ServiceDescriptor nonGeneric = CreateNonGenericJsonSerializerDescriptor(optionsAccessor: null, serviceLifetime);
    services.TryAdd(nonGeneric);

    ServiceDescriptor validator = new(
      typeof(NoticeValidator<TEnterpriseEventBaseType>),
      _ => new NoOpNoticeValidator<TEnterpriseEventBaseType>(),
      serviceLifetime
    );
    services.TryAdd(validator);


    ServiceDescriptor notifyOfOmittedConfigurationStreamSelector =
      new(
        typeof(NoticeStreamSelector<TEnterpriseEventBaseType>),
        _ => new InvalidOperationStreamSelector<TEnterpriseEventBaseType>(),
        serviceLifetime
      );
    services.TryAdd(notifyOfOmittedConfigurationStreamSelector);

    return this;
  }


  #region Routing

  public TypedNoticesBuilder<TEnterpriseEventBaseType> UseStreamSelector(
    NoticeStreamSelector<TEnterpriseEventBaseType> streamSelector
  )
  {
    services.Add(
      new ServiceDescriptor(typeof(NoticeStreamSelector<TEnterpriseEventBaseType>), streamSelector)
    );

    return this;
  }

  public TypedNoticesBuilder<TEnterpriseEventBaseType> UseStreamSelector(
    Func<IServiceProvider, NoticeStreamSelector<TEnterpriseEventBaseType>> factory,
    ServiceLifetime lifetime
  )
  {
    services.Add(
      new ServiceDescriptor(typeof(NoticeStreamSelector<TEnterpriseEventBaseType>), factory, lifetime)
    );

    return this;
  }

  /// <summary>
  ///   Configures the enterprise event builder to use a constant stream selector, routing all enterprise event notices to
  ///   the specified stream ID.
  /// </summary>
  /// <param name="stream">
  ///   An instance of <see cref="EventStreamId" /> that identifies the constant stream to be used.
  /// </param>
  /// <returns>
  ///   Returns the modified <see cref="TypedNoticesBuilder{TEnterpriseEventBaseType}" /> instance configured to use the
  ///   specified constant stream selector.
  /// </returns>
  public TypedNoticesBuilder<TEnterpriseEventBaseType> WithConstantStream(EventStreamId stream)
  {
    services.Add(
      new ServiceDescriptor(
        typeof(NoticeStreamSelector<TEnterpriseEventBaseType>),
        _ => new ConstantStreamSelector<TEnterpriseEventBaseType>(stream),
        ServiceLifetime.Singleton
      )
    );

    return this;
  }

  #endregion

  #region Serialization

  public TypedNoticesBuilder<TEnterpriseEventBaseType> UseSerializer(
    Func<IServiceProvider, NoticeSerializer<TEnterpriseEventBaseType>> factory,
    ServiceLifetime lifetime)
  {
    services.Add(
      new ServiceDescriptor(typeof(NoticeSerializer<TEnterpriseEventBaseType>), factory, lifetime)
    );

    return this;
  }

  public TypedNoticesBuilder<TEnterpriseEventBaseType> UseSerializer(
    NoticeSerializer<TEnterpriseEventBaseType> serializer)
  {
    services.Add(new ServiceDescriptor(typeof(NoticeSerializer<TEnterpriseEventBaseType>), serializer));

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
    ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
  {
    ServiceDescriptor descriptor = CreateJsonSerializerDescriptor(optionsAccessor, serviceLifetime);
    services.Add(descriptor);
    ServiceDescriptor nonGeneric = CreateNonGenericJsonSerializerDescriptor(optionsAccessor, serviceLifetime);
    services.Add(nonGeneric);

    return this;
  }

  private static ServiceDescriptor CreateJsonSerializerDescriptor(
    Func<IServiceProvider, JsonSerializerOptions>? optionsAccessor,
    ServiceLifetime serviceLifetime)
  {
    return new ServiceDescriptor(
      typeof(NoticeSerializer<TEnterpriseEventBaseType>),
      object (runtimeServiceProvider) =>
      {
        JsonSerializerOptions? possibleOptions = optionsAccessor?.Invoke(runtimeServiceProvider);
        JsonNoticeSerializer<TEnterpriseEventBaseType> func = new(possibleOptions);

        return func;
      },
      serviceLifetime
    );
  }

  private static ServiceDescriptor CreateNonGenericJsonSerializerDescriptor(
    Func<IServiceProvider, JsonSerializerOptions>? optionsAccessor,
    ServiceLifetime serviceLifetime)
  {
    return new ServiceDescriptor(
      typeof(NoticeSerializer),
      object (runtimeServiceProvider) =>
      {
        JsonSerializerOptions? possibleOptions = optionsAccessor?.Invoke(runtimeServiceProvider);
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
    services.Add(
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
    services.Add(
      new ServiceDescriptor(typeof(NoticeValidator<TEnterpriseEventBaseType>), validator)
    );

    return this;
  }

  #endregion
}
