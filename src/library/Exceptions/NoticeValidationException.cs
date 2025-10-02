namespace Jds.NiceNotice;

/// <summary>
///   Represents an exception thrown when a validation operation fails during the dispatch of an enterprise
///   event.
/// </summary>
public class NoticeValidationException : Exception
{
  /// <summary>
  ///   Initializes a new instance of the <see cref="NoticeValidationException" /> class.
  /// </summary>
  public NoticeValidationException(string message, Exception? innerException)
    : base(message, innerException)
  {
    ValidationFailures = [];
  }

  /// <summary>
  ///   Initializes a new instance of the <see cref="NoticeValidationException" /> class.
  /// </summary>
  public NoticeValidationException(string message, IReadOnlyList<string> validationFailures, Exception? innerException)
    : base(message, innerException)
  {
    ValidationFailures = validationFailures;
  }

  /// <summary>
  ///   Gets the validation failures that occurred during the dispatch of the enterprise event.
  /// </summary>
  public IReadOnlyList<string> ValidationFailures { get; }
}
