using Jds.NiceNotice.Configuration;
using Jds.NiceNotice.TypedNotices.Routing.Implementations;

using Microsoft.Extensions.DependencyInjection;

namespace Jds.NiceNotice.TypedNotices.Routing;

/// <summary>
///   Constructors for enterprise event stream selectors.
/// </summary>
public static class Routers
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
  public static NoticeRouter<TEnterpriseEventBaseType> Constant<TEnterpriseEventBaseType>(EventStreamId stream)
    where TEnterpriseEventBaseType : notnull
  {
    return new ConstantRouter<TEnterpriseEventBaseType>(stream);
  }

  /// <summary>
  ///   Creates a notice stream selector which uses the provided <paramref name="selector" /> to determine
  ///   each enterprise event's stream.
  /// </summary>
  /// <param name="selector">
  ///   The function which will determine the stream for each event.
  ///   This function is expected to be thread-safe.
  /// </param>
  /// <typeparam name="TEnterpriseEventBaseType">The base enterprise event type.</typeparam>
  /// <returns>Returns a notice stream selector.</returns>
  public static NoticeRouter<TEnterpriseEventBaseType> Delegate<TEnterpriseEventBaseType>(
    Func<TEnterpriseEventBaseType, EventStreamId> selector
  )
    where TEnterpriseEventBaseType : notnull
  {
    return new DelegateRouter<TEnterpriseEventBaseType>(selector);
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
  ///     will instead trigger a <see cref="NoticeRoutingException" />.
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
  public static NoticeRouter<TEnterpriseEventBaseType> TypeMap<TEnterpriseEventBaseType>(
    IReadOnlyDictionary<Type, EventStreamId> map,
    EventStreamId? defaultStream = null
  )
    where TEnterpriseEventBaseType : notnull
  {
    return new TypeMapRouter<TEnterpriseEventBaseType>(map, defaultStream);
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
  public static TypedNoticesBuilder<TEnterpriseEventBaseType> RouteToConstantStream<TEnterpriseEventBaseType>(
    this TypedNoticesBuilder<TEnterpriseEventBaseType> builder,
    EventStreamId stream
  )
    where TEnterpriseEventBaseType : notnull
  {
    return builder.UseStreamSelector(
      _ => Constant<TEnterpriseEventBaseType>(stream),
      ServiceLifetime.Singleton
    );
  }

  /// <summary>
  ///   Configures the enterprise event builder to use a delegate stream selector, invoking
  ///   <paramref name="routingFunction" /> for each notice.
  /// </summary>
  /// <param name="builder">The builder instance.</param>
  /// <param name="routingFunction">
  ///   The function which will determine the stream for each event.
  ///   This function is expected to be thread-safe.
  /// </param>
  /// <typeparam name="TEnterpriseEventBaseType">An enterprise event base type.</typeparam>
  /// <returns>
  ///   Returns the modified <see cref="TypedNoticesBuilder{TEnterpriseEventBaseType}" /> instance configured to use the
  ///   delegate stream selector.
  /// </returns>
  public static TypedNoticesBuilder<TEnterpriseEventBaseType> RouteWithDelegate<TEnterpriseEventBaseType>(
    this TypedNoticesBuilder<TEnterpriseEventBaseType> builder,
    Func<TEnterpriseEventBaseType, EventStreamId> routingFunction
  )
    where TEnterpriseEventBaseType : notnull
  {
    return builder.UseStreamSelector(
      _ => Delegate(routingFunction),
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
  public static TypedNoticesBuilder<TEnterpriseEventBaseType> RouteToTypeNameStreams<TEnterpriseEventBaseType>(
    this TypedNoticesBuilder<TEnterpriseEventBaseType> builder,
    bool useFullTypeName = false
  )
    where TEnterpriseEventBaseType : notnull
  {
    return builder.UseStreamSelector(useFullTypeName ? FullName : TypeName, ServiceLifetime.Singleton);

    static NoticeRouter<TEnterpriseEventBaseType> FullName(IServiceProvider _)
    {
      return FullNameFactory<TEnterpriseEventBaseType>();
    }

    static NoticeRouter<TEnterpriseEventBaseType> TypeName(IServiceProvider _)
    {
      return TypeNameFactory<TEnterpriseEventBaseType>();
    }
  }

  /// <summary>
  ///   Creates a notice stream selector which sends events to streams using their type name.
  /// </summary>
  /// <remarks>
  ///   This is a very common routing strategy.
  ///   Custom implementations of <see cref="INoticeIo" /> could use the <c>nameof</c> keyword
  ///   within a <c>switch</c> statement to connect logical event streams to their I/O destination (e.g., SNS topic).
  /// </remarks>
  /// <param name="useFullTypeName">
  ///   A value indicating whether the type name (e.g., <c>UserLogin</c>)
  ///   or the full type name (e.g., <c>MyOrganization.MyApp.UserLogin</c>)
  ///   should be used when determining the <see cref="EventStreamId" />.
  /// </param>
  /// <typeparam name="TEnterpriseEventBaseType">A base enterprise event notice type.</typeparam>
  /// <returns>Returns the constructed stream selector.</returns>
  public static NoticeRouter<TEnterpriseEventBaseType> TypeNameStreams<TEnterpriseEventBaseType>(
    bool useFullTypeName = false
  )
    where TEnterpriseEventBaseType : notnull
  {
    return useFullTypeName ? FullNameFactory<TEnterpriseEventBaseType>() : TypeNameFactory<TEnterpriseEventBaseType>();
  }

  private static NoticeRouter<TEnterpriseEventBaseType> FullNameFactory<TEnterpriseEventBaseType>()
    where TEnterpriseEventBaseType : notnull
  {
    return Delegate<TEnterpriseEventBaseType>(DelegateAlgorithms.AttributeOrFullNameStreamProvider);
  }

  private static NoticeRouter<TEnterpriseEventBaseType> TypeNameFactory<TEnterpriseEventBaseType>()
    where TEnterpriseEventBaseType : notnull
  {
    return Delegate<TEnterpriseEventBaseType>(DelegateAlgorithms.AttributeOrTypeNameStreamProvider);
  }

  internal static class DelegateAlgorithms
  {
    internal static EventStreamId AttributeOrFullNameStreamProvider<TEnterpriseEventBaseType>(
      TEnterpriseEventBaseType eventData
    )
      where TEnterpriseEventBaseType : notnull
    {
      Type type = eventData.GetType();

      return NoticeStreamAttributeHelpers.TryGetNoticeEventStreamId(type) ??
             (EventStreamId)(type.FullName ?? type.Name);
    }

    internal static EventStreamId AttributeOrTypeNameStreamProvider<TEnterpriseEventBaseType>(
      TEnterpriseEventBaseType eventData
    )
      where TEnterpriseEventBaseType : notnull
    {
      Type type = eventData.GetType();

      return NoticeStreamAttributeHelpers.TryGetNoticeEventStreamId(type) ??
             (EventStreamId)type.Name;
    }
  }
}
