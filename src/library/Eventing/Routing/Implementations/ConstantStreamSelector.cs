namespace Jds.NiceNotice;

/// <summary>
///   An implementation of <see cref="NoticeStreamSelector{TEnterpriseEventBaseType}" /> that always returns the same
///   stream: <paramref name="stream" />.
/// </summary>
/// <param name="stream">A stream. Will be the only stream id returned from <see cref="GetStreamId" />.</param>
/// <typeparam name="TEnterpriseEventBaseType">A base enterprise event notice type.</typeparam>
internal class ConstantStreamSelector<TEnterpriseEventBaseType>(EventStreamId stream)
  : NoticeStreamSelector<TEnterpriseEventBaseType> where TEnterpriseEventBaseType : notnull
{
  /// <inheritdoc />
  public override EventStreamId GetStreamId<TEventType>(TEventType notice)
  {
    return stream;
  }
}
