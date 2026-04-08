using System.ComponentModel.DataAnnotations;

namespace Jds.NiceNotice.TypedNotices.Validation;

/// <summary>
///   Helper methods for validating notifications using <see cref="System.ComponentModel.DataAnnotations.Validator" />.
/// </summary>
public static class DataAnnotationsValidation
{
  /// <summary>
  ///   Validate all properties of the notification using <see cref="System.ComponentModel.DataAnnotations.Validator" />.
  /// </summary>
  /// <param name="notification">An object to validate.</param>
  /// <typeparam name="TNotice">A notification type.</typeparam>
  /// <returns>Returns a list of validation errors, or <c>null</c> if validation succeeds.</returns>
  public static IReadOnlyList<string>? ValidateAllProperties<TNotice>(TNotice notification)
    where TNotice : notnull
  {
    try
    {
      List<ValidationResult> failures = [];
      bool isValid = Validator.TryValidateObject(notification, new ValidationContext(notification), failures);

      return isValid
        ? null
        : failures
          .Select(static r => $"{string.Join(separator: ",", r.MemberNames)}: {r.ErrorMessage}")
          .ToList();
    }
    catch (Exception e)
    {
      return
      [
        $"{notification.GetType().Name} Validation failed: {e}"
      ];
    }
  }
}
