using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.DependencyInjection;

namespace Jds.NiceNotice;

public static class DefaultEnterpriseEventValidators
{
  /// <summary>
  ///   Validate all properties of the notification using <see cref="System.ComponentModel.DataAnnotations.Validator" />.
  /// </summary>
  /// <param name="notification"></param>
  /// <typeparam name="TEnterpriseEventBaseType"></typeparam>
  /// <returns></returns>
  public static IReadOnlyList<string>? ValidateAll<TEnterpriseEventBaseType>(TEnterpriseEventBaseType notification)
    where TEnterpriseEventBaseType : notnull
  {
    List<ValidationResult> failures = [];
    bool isValid = Validator.TryValidateObject(notification, new ValidationContext(notification), failures);

    return isValid
      ? null
      : failures
        .Select(static r => $"{string.Join(separator: ",", r.MemberNames)}: {r.ErrorMessage}")
        .ToList();
  }

  public static IReadOnlyList<string>? NoValidation<TEnterpriseEventBaseType>(
    TEnterpriseEventBaseType notification,
    string serializedNotification)
  {
    return null;
  }

  public static NoticeValidator<TEnterpriseEventBaseType> DataAnnotationsValidator<TEnterpriseEventBaseType>()
    where TEnterpriseEventBaseType : notnull
  {
    return new FunctionNoticeValidator<TEnterpriseEventBaseType>(static (type, s) => ValidateAll(type));
  }

  public static NoticeValidator<TEnterpriseEventBaseType> NoOpValidator<TEnterpriseEventBaseType>()
    where TEnterpriseEventBaseType : notnull
  {
    return new NoOpNoticeValidator<TEnterpriseEventBaseType>();
  }

  public static TypedNoticesBuilder<TEnterpriseEventBaseType> WithDataAnnotationsValidator<TEnterpriseEventBaseType>(
    this TypedNoticesBuilder<TEnterpriseEventBaseType> builder)
    where TEnterpriseEventBaseType : notnull
  {
    return builder.UseValidator(
      static _ => DataAnnotationsValidator<TEnterpriseEventBaseType>(),
      ServiceLifetime.Singleton
    );
  }

  public static TypedNoticesBuilder<TEnterpriseEventBaseType> WithNoValidation<TEnterpriseEventBaseType>(
    this TypedNoticesBuilder<TEnterpriseEventBaseType> builder)
    where TEnterpriseEventBaseType : notnull
  {
    return builder.UseValidator(
      static _ => new NoOpNoticeValidator<TEnterpriseEventBaseType>(),
      ServiceLifetime.Singleton
    );
  }
}
