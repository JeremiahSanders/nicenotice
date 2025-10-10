using System.ComponentModel.DataAnnotations;

namespace Jds.NiceNotice;

/// <summary>
///   An implementation of <see cref="NoticeValidator{TEnterpriseEventBaseType}" /> that uses
///   a delegate to validate the enterprise event notice.
/// </summary>
/// <remarks>
///   <para>
///     Consider using <see cref="DataAnnotationsValidator.ValidateAllProperties{TNotice}" /> when implementing your
///     custom logic. It uses <see cref="Validator" /> to validate the notice.
///   </para>
/// </remarks>
/// <typeparam name="TEnterpriseEventBaseType">A base notification type.</typeparam>
/// <param name="validator">
///   A function which validates notices.
///   It should return <c>null</c> if the notice is valid, or a list of error messages if it is not.
/// </param>
public class FunctionNoticeValidator<TEnterpriseEventBaseType>(
  Func<TEnterpriseEventBaseType, string, IReadOnlyList<string>?> validator
) : NoticeValidator<TEnterpriseEventBaseType>
  where TEnterpriseEventBaseType : notnull
{
  /// <inheritdoc />
  public override IReadOnlyList<string>? Validate<TEventType>(TEventType notice, string serializedNotice)
  {
    return validator(notice, serializedNotice);
  }
}

/// <summary>
///   An implementation of <see cref="NoticeValidator" /> that uses
///   a delegate to validate the enterprise event notice.
/// </summary>
/// <remarks>
///   <para>
///     Consider using <see cref="DataAnnotationsValidator.ValidateAllProperties{TNotice}" /> when implementing your
///     custom logic. It uses <see cref="Validator" /> to validate the notice.
///   </para>
/// </remarks>
/// <param name="validator">
///   A function which validates notices.
///   It should return <c>null</c> if the notice is valid, or a list of error messages if it is not.
/// </param>
public class FunctionNoticeValidator(
  Func<object, string, IReadOnlyList<string>?> validator
) : NoticeValidator
{
  /// <inheritdoc />
  public override IReadOnlyList<string>? Validate<TEventType>(TEventType notice, string serializedNotice)
  {
    return validator(notice, serializedNotice);
  }
}
