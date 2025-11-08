using Jds.NiceNotice.Configuration;
using Jds.NiceNotice.TypedNotices.Validation.Implementations;

using Microsoft.Extensions.DependencyInjection;

namespace Jds.NiceNotice.TypedNotices.Validation;

/// <summary>
///   Methods for creating notification validators.
/// </summary>
public static class Validators
{
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
  ///   Creates a validator that uses data annotations to validate
  ///   typed notices (i.e., notification data transfer objects).
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     This validator does not constrain notifications to a base enterprise event type.
  ///     For typed enterprise events, use <see cref="DataAnnotationsValidator{TEnterpriseEventBaseType}" />.
  ///   </para>
  /// </remarks>
  /// <returns>Returns the created validator.</returns>
  public static NoticeValidator DataAnnotationsValidator()
  {
    return new DataAnnotationsValidator();
  }

  /// <summary>
  ///   Creates a validator that uses a function (<paramref name="validatorFunction" />) to validate enterprise events.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     This is a very useful validator.
  ///     It is a simpler alternative to implementing a custom <see cref="NoticeValidator{TEnterpriseEventBaseType}" />.
  ///   </para>
  /// </remarks>
  /// <param name="validatorFunction">
  ///   <para>
  ///     A custom validation logic function.
  ///     Use of <c>static</c> delegates is highly recommended.
  ///   </para>
  ///   <para>
  ///     The function receives the enterprise event to be validated and is expected to return <c>null</c>
  ///     if the event is valid, or a list of validation errors if the event is invalid.
  ///   </para>
  /// </param>
  /// <typeparam name="TEnterpriseEventBaseType">A base type for the application's enterprise event notifications.</typeparam>
  /// <returns>Returns the created validator.</returns>
  public static NoticeValidator<TEnterpriseEventBaseType> DelegateValidator<TEnterpriseEventBaseType>(
    Func<TEnterpriseEventBaseType, string, IReadOnlyList<string>?> validatorFunction)
    where TEnterpriseEventBaseType : notnull
  {
    return new DelegateNoticeValidator<TEnterpriseEventBaseType>(validatorFunction);
  }

  /// <summary>
  ///   Creates a validator that uses a function (<paramref name="validatorFunction" />) to validate enterprise events.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     This is a very useful validator.
  ///     It is a simpler alternative to implementing a custom <see cref="NoticeValidator" />.
  ///   </para>
  /// </remarks>
  /// <param name="validatorFunction">
  ///   <para>
  ///     A custom validation logic function.
  ///     Use of <c>static</c> delegates is highly recommended.
  ///   </para>
  ///   <para>
  ///     The function receives the enterprise event to be validated and is expected to return <c>null</c>
  ///     if the event is valid, or a list of validation errors if the event is invalid.
  ///   </para>
  /// </param>
  /// <returns>Returns the created validator.</returns>
  public static NoticeValidator DelegateValidator(
    Func<object, string, IReadOnlyList<string>?> validatorFunction)
  {
    return new DelegateNoticeValidator(validatorFunction);
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
  public static TypedNoticesBuilder<TEnterpriseEventBaseType> ValidateWithDataAnnotations<TEnterpriseEventBaseType>(
    this TypedNoticesBuilder<TEnterpriseEventBaseType> builder)
    where TEnterpriseEventBaseType : notnull
  {
    return builder.UseValidator(
      static _ => DataAnnotationsValidator<TEnterpriseEventBaseType>(),
      ServiceLifetime.Singleton
    );
  }

  /// <summary>
  ///   Configures this typed notice builder to use a validator that executes a custom validation function,
  ///   <paramref name="genericValidatorFunction" />.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     This is the easiest way to add custom validation logic to enterprise event notifications.
  ///     Provide a <c>static</c> delegate <paramref name="genericValidatorFunction" /> and it will be called for each
  ///     enterprise event dispatched.
  ///   </para>
  ///   <para>
  ///     The use of two validator functions,
  ///     <paramref name="genericValidatorFunction" /> and <paramref name="untypedValidatorFunction" />,
  ///     is needed to support the breadth of typed notification dispatching interfaces.
  ///   </para>
  /// </remarks>
  /// <param name="builder">This typed notice builder.</param>
  /// <param name="genericValidatorFunction">
  ///   <para>
  ///     A custom validation logic function which is invoked from
  ///     <see cref="ITypedNoticeDispatcher{TEnterpriseEventBaseType}" />.
  ///     Use of <c>static</c> delegates is highly recommended.
  ///   </para>
  ///   <para>
  ///     The function receives the enterprise event to be validated and is expected to return <c>null</c>
  ///     if the event is valid, or a list of validation errors if the event is invalid.
  ///   </para>
  /// </param>
  /// <param name="untypedValidatorFunction">
  ///   <para>
  ///     A custom validation logic function which is invoked from <see cref="ITypedNoticeDispatcher" />.
  ///     Use of <c>static</c> delegates is highly recommended.
  ///   </para>
  ///   <para>
  ///     The function receives the enterprise event to be validated and is expected to return <c>null</c>
  ///     if the event is valid, or a list of validation errors if the event is invalid.
  ///   </para>
  /// </param>
  /// <typeparam name="TEnterpriseEventBaseType">A base type for the application's enterprise event notifications.</typeparam>
  /// <returns>Returns this typed notice builder for further configuration.</returns>
  public static TypedNoticesBuilder<TEnterpriseEventBaseType> ValidateWithDelegate<TEnterpriseEventBaseType>(
    this TypedNoticesBuilder<TEnterpriseEventBaseType> builder,
    Func<TEnterpriseEventBaseType, string, IReadOnlyList<string>?> genericValidatorFunction,
    Func<object, string, IReadOnlyList<string>?> untypedValidatorFunction
  )
    where TEnterpriseEventBaseType : notnull
  {
    // Register the generic validator function.
    builder.UseValidator(
      _ => DelegateValidator(genericValidatorFunction),
      ServiceLifetime.Singleton
    );

    // Register the untyped validator function.
    builder.Services.Add(
      new ServiceDescriptor(
        typeof(NoticeValidator),
        _ => new DelegateNoticeValidator(untypedValidatorFunction),
        ServiceLifetime.Singleton
      )
    );

    return builder;
  }

  /// <summary>
  ///   Configures this typed notice builder to use a validator that performs no validation;
  ///   all enterprise events are considered valid.
  /// </summary>
  /// <param name="builder">This typed notice builder.</param>
  /// <typeparam name="TEnterpriseEventBaseType">A base type for the application's enterprise event notifications.</typeparam>
  /// <returns>Returns this typed notice builder for further configuration.</returns>
  public static TypedNoticesBuilder<TEnterpriseEventBaseType> ValidateNothing<TEnterpriseEventBaseType>(
    this TypedNoticesBuilder<TEnterpriseEventBaseType> builder)
    where TEnterpriseEventBaseType : notnull
  {
    return builder.UseValidator(
      static _ => NoOpValidator<TEnterpriseEventBaseType>(),
      ServiceLifetime.Singleton
    );
  }
}
