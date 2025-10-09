using System.ComponentModel.DataAnnotations;

namespace Jds.NiceNotice;

public class DataAnnotationsValidator<TEnterpriseEventBaseType> : NoticeValidator<TEnterpriseEventBaseType>
  where TEnterpriseEventBaseType : notnull
{
  public override IReadOnlyList<string>? Validate<TEventType>(TEventType notice, string serializedNotice)
  {
    return DataAnnotationsValidator.ValidateAllProperties(notice);
  }
}

public class DataAnnotationsValidator : NoticeValidator
{
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
