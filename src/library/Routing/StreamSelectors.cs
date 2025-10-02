namespace Jds.NiceNotice;

/// <summary>
///   Constructors for enterprise event stream selectors.
/// </summary>
public static class StreamSelectors
{
  /// <summary>
  ///   Creates a notice stream selector which sends all events to the same <paramref name="stream" />.
  /// </summary>
  /// <remarks>
  ///   <para>Use this stream selector when events should all be routed to the same destination (e.g., AWS SNS topic).</para>
  ///   <para>
  ///     If any events need to be routed to alternate destinations:
  ///     use <see cref="TypeMap{TEnterpriseEventBaseType}" /> to route events based upon an event type to destination map,
  ///     or use <see cref="Delegate{TEnterpriseEventBaseType}" /> to provide custom routing logic.
  ///   </para>
  /// </remarks>
  /// <param name="stream">The stream identifier to which all events will be routed.</param>
  /// <typeparam name="TEnterpriseEventBaseType">The base enterprise event type.</typeparam>
  /// <returns>Returns a notice stream selector.</returns>
  public static NoticeStreamSelector<TEnterpriseEventBaseType> Constant<TEnterpriseEventBaseType>(EventStreamId stream)
    where TEnterpriseEventBaseType : notnull
  {
    return new ConstantStreamSelector<TEnterpriseEventBaseType>(stream);
  }

  /// <summary>
  ///   Creates a notice stream selector which uses the provided <paramref name="selector" /> to determine
  ///   each enterprise event's stream.
  /// </summary>
  /// <param name="selector">The function which will determine the stream for each event.</param>
  /// <typeparam name="TEnterpriseEventBaseType">The base enterprise event type.</typeparam>
  /// <returns>Returns a notice stream selector.</returns>
  public static NoticeStreamSelector<TEnterpriseEventBaseType> Delegate<TEnterpriseEventBaseType>(
    Func<TEnterpriseEventBaseType, EventStreamId> selector) where TEnterpriseEventBaseType : notnull
  {
    return new DelegateStreamSelector<TEnterpriseEventBaseType>(selector);
  }

  /// <summary>
  ///   Creates a notice stream selector which uses the provided type <paramref name="map" />
  ///   and optional <paramref name="defaultStream" /> to specify where events should be routed.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Use this stream selector to declaratively define event routing.
  ///   </para>
  ///   <para>
  ///     It is strongly recommended to use the <paramref name="defaultStream" /> to ensure that all events have a
  ///     destination. Without a default, dispatching any event types that are not included in <paramref name="map" />
  ///     will instead trigger a <see cref="StreamDeterminationException" />.
  ///   </para>
  /// </remarks>
  /// <param name="map">
  ///   The type map which defines where each type
  ///   (assumed to be derived from <typeparamref name="TEnterpriseEventBaseType" />)
  ///   should be routed.
  /// </param>
  /// <param name="defaultStream">The default stream which should receive all events not included in <paramref name="map" />.</param>
  /// <typeparam name="TEnterpriseEventBaseType">The base enterprise event type.</typeparam>
  /// <returns>Returns a notice stream selector.</returns>
  public static NoticeStreamSelector<TEnterpriseEventBaseType> TypeMap<TEnterpriseEventBaseType>(
    IReadOnlyDictionary<Type, EventStreamId> map,
    EventStreamId? defaultStream = null) where TEnterpriseEventBaseType : notnull
  {
    return new TypeMapStreamSelector<TEnterpriseEventBaseType>(map, defaultStream);
  }
}
