namespace Jds.NiceNotice.TypedNotices.Routing;

/// <summary>
///   Represents an exception which is thrown when unable to determine the event stream to which an enterprise event
///   should be dispatched.
/// </summary>
public class NoticeRoutingException(string message, Exception? innerException)
  : Exception(message, innerException)
{
}
