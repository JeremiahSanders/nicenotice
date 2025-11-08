namespace Jds.NiceNotice.TypedNotices.Validation.Implementations;

/// <summary>
///   An implementation of <see cref="NoticeValidator{TEnterpriseEventBaseType}" /> that uses
///   <see cref="System.ComponentModel.DataAnnotations.Validator" /> to validate the enterprise event notice.
/// </summary>
/// <typeparam name="TEnterpriseEventBaseType">A base notification type.</typeparam>
internal class DataAnnotationsValidator<TEnterpriseEventBaseType> : NoticeValidator<TEnterpriseEventBaseType>
  where TEnterpriseEventBaseType : notnull
{
  /// <inheritdoc />
  public override IReadOnlyList<string>? Validate<TEventType>(TEventType notice, string serializedNotice)
  {
    return DataAnnotationsValidation.ValidateAllProperties(notice);
  }
}

/// <summary>
///   An implementation of <see cref="NoticeValidator" /> that uses
///   <see cref="System.ComponentModel.DataAnnotations.Validator" /> to validate the enterprise event notice.
/// </summary>
internal class DataAnnotationsValidator : NoticeValidator
{
  /// <inheritdoc />
  public override IReadOnlyList<string>? Validate<TEventType>(TEventType notice, string serializedNotice)
  {
    return DataAnnotationsValidation.ValidateAllProperties(notice);
  }
}
