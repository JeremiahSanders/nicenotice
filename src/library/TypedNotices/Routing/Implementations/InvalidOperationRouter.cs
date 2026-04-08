namespace Jds.NiceNotice.TypedNotices.Routing.Implementations;

/// <summary>
///   A notice stream selector which always throws an <see cref="InvalidOperationException" />.
/// </summary>
/// <typeparam name="TEnterpriseEventBaseType">A base enterprise event type.</typeparam>
internal class InvalidOperationRouter<TEnterpriseEventBaseType> : NoticeRouter<TEnterpriseEventBaseType>
  where TEnterpriseEventBaseType : notnull
{
  /// <inheritdoc />
  public override EventStreamId GetStreamId<TEventType>(TEventType notice)
  {
    throw new InvalidOperationException(
      $"No enterprise event stream configured for {typeof(TEnterpriseEventBaseType).Name}."
    );
  }
}
