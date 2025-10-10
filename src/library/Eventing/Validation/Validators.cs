using Microsoft.Extensions.DependencyInjection;

namespace Jds.NiceNotice;

/// <summary>
///   Methods for creating notification validators.
/// </summary>
public static class Validators
{
  // TODO: Add helpers for FunctionNoticeValidator.

  /// <summary>
  ///   Creates a validator that uses data annotations to validate enterprise events.
  /// </summary>
  /// <typeparam name="TEnterpriseEventBaseType">A base type for the application's enterprise event notifications.</typeparam>
  /// <returns>Returns the created validator.</returns>
  public static NoticeValidator<TEnterpriseEventBaseType> DataAnnotationsValidator<TEnterpriseEventBaseType>()
    where TEnterpriseEventBaseType : notnull
  {
    return new DataAnnotationsValidator<TEnterpriseEventBaseType>();
  }

  /// <summary>
  ///   Creates a validator that performs no validation; all enterprise events are considered valid.
  /// </summary>
  /// <typeparam name="TEnterpriseEventBaseType">A base type for the application's enterprise event notifications.</typeparam>
  /// <returns>Returns the created validator.</returns>
  public static NoticeValidator<TEnterpriseEventBaseType> NoOpValidator<TEnterpriseEventBaseType>()
    where TEnterpriseEventBaseType : notnull
  {
    return new NoOpNoticeValidator<TEnterpriseEventBaseType>();
  }

  /// <summary>
  ///   Configures this typed notice builder to use a validator that uses data annotations to validate enterprise events.
  ///   (See <see cref="DataAnnotationsValidator{TEnterpriseEventBaseType}" />)
  /// </summary>
  /// <param name="builder">This typed notice builder.</param>
  /// <typeparam name="TEnterpriseEventBaseType">A base type for the application's enterprise event notifications.</typeparam>
  /// <returns>Returns this typed notice builder for further configuration.</returns>
  public static TypedNoticesBuilder<TEnterpriseEventBaseType> WithDataAnnotationsValidator<TEnterpriseEventBaseType>(
    this TypedNoticesBuilder<TEnterpriseEventBaseType> builder)
    where TEnterpriseEventBaseType : notnull
  {
    return builder.UseValidator(
      static _ => DataAnnotationsValidator<TEnterpriseEventBaseType>(),
      ServiceLifetime.Singleton
    );
  }

  /// <summary>
  ///   Configures this typed notice builder to use a validator that performs no validation;
  ///   all enterprise events are considered valid.
  /// </summary>
  /// <param name="builder">This typed notice builder.</param>
  /// <typeparam name="TEnterpriseEventBaseType">A base type for the application's enterprise event notifications.</typeparam>
  /// <returns>Returns this typed notice builder for further configuration.</returns>
  public static TypedNoticesBuilder<TEnterpriseEventBaseType> WithNoValidation<TEnterpriseEventBaseType>(
    this TypedNoticesBuilder<TEnterpriseEventBaseType> builder)
    where TEnterpriseEventBaseType : notnull
  {
    return builder.UseValidator(
      static _ => NoOpValidator<TEnterpriseEventBaseType>(),
      ServiceLifetime.Singleton
    );
  }
}
