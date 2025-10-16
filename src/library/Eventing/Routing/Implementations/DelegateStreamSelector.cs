namespace Jds.NiceNotice;

/// <summary>
///   An implementation of <see cref="NoticeStreamSelector{TEnterpriseEventBaseType}" /> that delegates
///   <see cref="GetStreamId" /> to a delegate: <paramref name="selector" />.
/// </summary>
/// <param name="selector">A delegate to invoke. Expected to be thread safe.</param>
/// <typeparam name="TEnterpriseEventBaseType">A base enterprise event notice type.</typeparam>
internal class DelegateStreamSelector<TEnterpriseEventBaseType>(Func<TEnterpriseEventBaseType, EventStreamId> selector)
  : NoticeStreamSelector<TEnterpriseEventBaseType> where TEnterpriseEventBaseType : notnull
{
  /// <inheritdoc />
  public override EventStreamId GetStreamId<TEventType>(TEventType notice)
  {
    return selector(notice);
  }
}
