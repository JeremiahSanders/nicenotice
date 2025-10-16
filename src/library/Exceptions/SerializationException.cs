namespace Jds.NiceNotice;

/// <summary>
///   Represents an exception which is thrown when a failure occurs during the serialization of an enterprise event.
/// </summary>
public class SerializationException(string message, Exception? innerException) : Exception(message, innerException)
{
}
