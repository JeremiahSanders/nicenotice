using System.ComponentModel.DataAnnotations;

namespace Jds.NiceNotice;

public static class DefaultEnterpriseEventValidators
{
  public static IReadOnlyList<string>? ValidateAll<TEnterpriseEventBaseType>(TEnterpriseEventBaseType notification)
    where TEnterpriseEventBaseType : notnull
  {
    List<ValidationResult> failures = [];
    bool isValid = Validator.TryValidateObject(notification, new ValidationContext(notification), failures);

    return isValid
      ? null
      : failures
        .Select(r => $"{string.Join(separator: ",", r.MemberNames)}: {r.ErrorMessage}")
        .ToList();
  }

  public static IReadOnlyList<string>? NoValidation<TEnterpriseEventBaseType>(
    TEnterpriseEventBaseType notification,
    string serializedNotification)
  {
    return null;
  }
}
