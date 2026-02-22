namespace Jds.NiceNotice;

/// <summary>
///   Represents a unique identifier for an event stream. This type is used to uniquely identify and manage event streams,
///   ensuring type safety when working with specific streams in the application's event notification system.
/// </summary>
/// <remarks>
///   <para>
///     Event stream ids are a thin wrapper around a string intended to identify a single conceptual stream of messages.
///     One can think of an event stream as the &quot;door&quot; or &quot;path&quot; the message follows.
///     Individual applications determine how many distinct event streams their messages require.
///   </para>
///   <para>
///     Some applications have a single output notification stream for all messages.
///     This is the place to start; all applications will need at least one.
///   </para>
///   <para>
///     Other applications route messages to distinct &quot;paths&quot; or &quot;channels&quot; based upon where they need
///     to go or who needs to react to them.
///     For example, they may have a stream for communicating metric-style notifications,
///     or a stream for communicating state changes of managed data (e.g., in response to CRUD operations),
///     or maybe even a stream specifically for dispatching error notifications.
///   </para>
///   <para>
///     For a further example,
///     Console applications inherently have two conceptual notification streams provided by the operating system:
///     standard out (<c>stdout</c>) and standard error (<c>stderr</c>).
///     (This is only a comparison; there are no predefined console event stream ids.)
///   </para>
/// </remarks>
public readonly record struct EventStreamId
{
  private readonly string _value;

  private EventStreamId(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException(message: "Event stream ID cannot be empty.", nameof(value));
    }

    _value = value;
  }

  /// <summary>
  ///   Static constructor for creating a new event stream id from a string value.
  /// </summary>
  /// <param name="value">An event stream id value.</param>
  /// <returns>Returns the event stream id derived from <paramref name="value" />.</returns>
  public static EventStreamId From(string value)
  {
    return new EventStreamId(value);
  }

  /// <summary>
  ///   Converts a string <paramref name="value" /> to an <see cref="EventStreamId" />.
  /// </summary>
  /// <remarks>Explicit conversion from string reinforces conscious decisions about creation.</remarks>
  /// <param name="value">An event stream id value.</param>
  /// <returns>Returns the created <see cref="EventStreamId" />.</returns>
  public static explicit operator EventStreamId(string value)
  {
    return From(value);
  }

  /// <summary>
  ///   Returns the string value of an <see cref="EventStreamId" />.
  /// </summary>
  /// <param name="id">An event stream id.</param>
  /// <returns>Returns the string value of <paramref name="id" />.</returns>
  public static implicit operator string(EventStreamId id)
  {
    return id._value;
  }

  /// <summary>
  ///   Returns the string value of this <see cref="EventStreamId" />.
  /// </summary>
  /// <returns>Returns this instance's value.</returns>
  public override string ToString()
  {
    return _value;
  }
}
