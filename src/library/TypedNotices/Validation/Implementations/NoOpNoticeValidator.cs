namespace Jds.NiceNotice.TypedNotices.Validation.Implementations;

/// <summary>
///   A notice validator that does not perform any validation (all notices are considered valid).
/// </summary>
internal class NoOpNoticeValidator<TEnterpriseEventBaseType> : NoticeValidator<TEnterpriseEventBaseType>
  where TEnterpriseEventBaseType : notnull
{
  /// <inheritdoc />
  /// <remarks>This implementation does not perform any validation.</remarks>
  public override IReadOnlyList<string>? Validate<TEventType>(TEventType notice, string serializedNotice)
  {
    return null;
  }
}

/// <summary>
///   A notice validator that does not perform any validation (all notices are considered valid).
/// </summary>
internal class NoOpNoticeValidator : NoticeValidator
{
  /// <inheritdoc />
  /// <remarks>This implementation does not perform any validation.</remarks>
  public override IReadOnlyList<string>? Validate<TEventType>(TEventType notice, string serializedNotice)
  {
    return null;
  }
}
