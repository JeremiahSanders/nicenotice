using Microsoft.Extensions.DependencyInjection;

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


  /// <summary>
  ///   Configures the enterprise event builder to use a constant stream selector, routing all enterprise event notices to
  ///   the specified stream ID.
  /// </summary>
  /// <param name="builder">The builder instance.</param>
  /// <param name="stream">
  ///   An instance of <see cref="EventStreamId" /> that identifies the constant stream to be used.
  /// </param>
  /// <typeparam name="TEnterpriseEventBaseType">An enterprise event base type.</typeparam>
  /// <returns>
  ///   Returns the modified <see cref="TypedNoticesBuilder{TEnterpriseEventBaseType}" /> instance configured to use the
  ///   specified constant stream selector.
  /// </returns>
  public static TypedNoticesBuilder<TEnterpriseEventBaseType> WithConstantStream<TEnterpriseEventBaseType>(
    this TypedNoticesBuilder<TEnterpriseEventBaseType> builder,
    EventStreamId stream
  ) where TEnterpriseEventBaseType : notnull
  {
    return builder.UseStreamSelector(
      _ => Constant<TEnterpriseEventBaseType>(stream),
      ServiceLifetime.Singleton
    );
  }

  /// <summary>
  ///   Configures the enterprise event builder to use a stream selector which routes each enterprise event to a stream
  ///   having the same name as the event's type.
  ///   (E.g., a type named "MyEvent" will be routed to a stream named <c>MyEvent</c>.)
  /// </summary>
  /// <param name="builder">The builder instance.</param>
  /// <param name="useFullTypeName">
  ///   A value indicating whether the full name
  ///   (i.e., including namespace, e.g., <c>MyCompany.MyApplication.MyEvent</c>)
  ///   or the simple name
  ///   (i.e., without namespace, e.g., <c>MyEvent</c>)
  ///   should be used to determine the stream name.
  ///   Defaults to <c>false</c>.
  /// </param>
  /// <typeparam name="TEnterpriseEventBaseType">An enterprise event base type.</typeparam>
  /// <returns>Returns the builder after modification.</returns>
  public static TypedNoticesBuilder<TEnterpriseEventBaseType> WithTypeNameStreams<TEnterpriseEventBaseType>(
    this TypedNoticesBuilder<TEnterpriseEventBaseType> builder,
    bool useFullTypeName = false) where TEnterpriseEventBaseType : notnull
  {
    return builder.UseStreamSelector(useFullTypeName ? FullNameFactory : TypeNameFactory, ServiceLifetime.Singleton);

    static NoticeStreamSelector<TEnterpriseEventBaseType> FullNameFactory(IServiceProvider _)
    {
      return Delegate<TEnterpriseEventBaseType>(static eventData => (EventStreamId)(eventData.GetType()
          .FullName ?? eventData.GetType()
          .Name)
      );
    }

    static NoticeStreamSelector<TEnterpriseEventBaseType> TypeNameFactory(IServiceProvider _)
    {
      return Delegate<TEnterpriseEventBaseType>(static eventData => (EventStreamId)eventData.GetType()
        .Name
      );
    }
  }
}
