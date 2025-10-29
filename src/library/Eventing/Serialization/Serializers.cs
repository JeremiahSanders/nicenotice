using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;

namespace Jds.NiceNotice;

/// <summary>
///   Methods arranging implementations of <see cref="NoticeSerializer" />.
/// </summary>
public static class Serializers
{
  /// <summary>
  ///   Creates a <see cref="NoticeSerializer" /> which serializes to JSON.
  /// </summary>
  /// <param name="options">
  ///   Optional. JSON serializer options. Default:
  ///   <see cref="JsonDefaults.DefaultJsonSerializerOptions" />
  /// </param>
  /// <returns>Returns the created serializer.</returns>
  public static NoticeSerializer Json(JsonSerializerOptions? options = null)
  {
    return new JsonNoticeSerializer(options);
  }

  /// <summary>
  ///   Configures the enterprise event builder to use a JSON serializer for serializing events.
  /// </summary>
  /// <param name="builder">A typed notices builder.</param>
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
  public static TypedNoticesBuilder<TEnterpriseEventBaseType> SerializeToJson<TEnterpriseEventBaseType>(
    this TypedNoticesBuilder<TEnterpriseEventBaseType> builder,
    Func<IServiceProvider, JsonSerializerOptions>? optionsAccessor = null,
    ServiceLifetime serviceLifetime = ServiceLifetime.Singleton
  ) where TEnterpriseEventBaseType : notnull
  {
    ServiceDescriptor descriptor =
      CreateJsonSerializerDescriptor<TEnterpriseEventBaseType>(optionsAccessor, serviceLifetime);
    builder.Services.Add(descriptor);
    ServiceDescriptor nonGeneric = CreateNonGenericJsonSerializerDescriptor(optionsAccessor, serviceLifetime);
    builder.Services.Add(nonGeneric);

    return builder;
  }

  internal static ServiceDescriptor CreateJsonSerializerDescriptor<TEnterpriseEventBaseType>(
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

  internal static ServiceDescriptor CreateNonGenericJsonSerializerDescriptor(
    Func<IServiceProvider, JsonSerializerOptions>? optionsAccessor,
    ServiceLifetime serviceLifetime)
  {
    return new ServiceDescriptor(
      typeof(NoticeSerializer),
      object (runtimeServiceProvider) =>
      {
        JsonSerializerOptions? possibleOptions = optionsAccessor?.Invoke(runtimeServiceProvider) ??
                                                 runtimeServiceProvider.GetService<JsonSerializerOptions>();

        return Json(possibleOptions);
      },
      serviceLifetime
    );
  }
}
