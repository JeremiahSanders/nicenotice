namespace Jds.NiceNotice;

/// <summary>
///   An implementation of <see cref="NoticeValidator{TEnterpriseEventBaseType}" /> that uses
///   a delegate to validate the enterprise event notice.
/// </summary>
/// <typeparam name="TEnterpriseEventBaseType">A base notification type.</typeparam>
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
