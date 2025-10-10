using System.ComponentModel.DataAnnotations;

namespace Jds.NiceNotice;

/// <summary>
///   An implementation of <see cref="NoticeValidator{TEnterpriseEventBaseType}" /> that uses
///   <see cref="System.ComponentModel.DataAnnotations.Validator" /> to validate the enterprise event notice.
/// </summary>
/// <typeparam name="TEnterpriseEventBaseType">A base notification type.</typeparam>
public class DataAnnotationsValidator<TEnterpriseEventBaseType> : NoticeValidator<TEnterpriseEventBaseType>
  where TEnterpriseEventBaseType : notnull
{
  /// <inheritdoc />
  public override IReadOnlyList<string>? Validate<TEventType>(TEventType notice, string serializedNotice)
  {
    return DataAnnotationsValidator.ValidateAllProperties(notice);
  }
}

/// <summary>
///   An implementation of <see cref="NoticeValidator" /> that uses
///   <see cref="System.ComponentModel.DataAnnotations.Validator" /> to validate the enterprise event notice.
/// </summary>
public class DataAnnotationsValidator : NoticeValidator
{
  /// <inheritdoc />
  public override IReadOnlyList<string>? Validate<TEventType>(TEventType notice, string serializedNotice)
  {
    return ValidateAllProperties(notice);
  }

  /// <summary>
  ///   Validate all properties of the notification using <see cref="System.ComponentModel.DataAnnotations.Validator" />.
  /// </summary>
  /// <param name="notification"></param>
  /// <typeparam name="TNotice"></typeparam>
  /// <returns></returns>
  public static IReadOnlyList<string>? ValidateAllProperties<TNotice>(TNotice notification)
    where TNotice : notnull
  {
    List<ValidationResult> failures = [];
    bool isValid = Validator.TryValidateObject(notification, new ValidationContext(notification), failures);

    return isValid
      ? null
      : failures
        .Select(static r => $"{string.Join(separator: ",", r.MemberNames)}: {r.ErrorMessage}")
        .ToList();
  }
}
