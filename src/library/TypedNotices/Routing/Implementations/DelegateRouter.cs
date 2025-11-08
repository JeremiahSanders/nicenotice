namespace Jds.NiceNotice.TypedNotices.Routing.Implementations;

/// <summary>
///   An implementation of <see cref="NoticeRouter{TEnterpriseEventBaseType}" /> that delegates
///   <see cref="GetStreamId" /> to a delegate: <paramref name="selector" />.
/// </summary>
/// <param name="selector">A delegate to invoke. Expected to be thread safe.</param>
/// <typeparam name="TEnterpriseEventBaseType">A base enterprise event notice type.</typeparam>
internal class DelegateRouter<TEnterpriseEventBaseType>(Func<TEnterpriseEventBaseType, EventStreamId> selector)
  : NoticeRouter<TEnterpriseEventBaseType> where TEnterpriseEventBaseType : notnull
{
  /// <inheritdoc />
  public override EventStreamId GetStreamId<TEventType>(TEventType notice)
  {
    return selector(notice);
  }
}
