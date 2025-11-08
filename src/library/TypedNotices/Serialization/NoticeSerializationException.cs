namespace Jds.NiceNotice.TypedNotices.Serialization;

/// <summary>
///   Represents an exception which is thrown when a failure occurs during the serialization of a typed notice.
/// </summary>
public class NoticeSerializationException(string message, Exception? innerException)
  : Exception(message, innerException)
{
}
