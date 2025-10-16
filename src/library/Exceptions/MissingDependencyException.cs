namespace Jds.NiceNotice;

/// <summary>
///   Represents an exception thrown when a required dependency is missing.
/// </summary>
public class MissingDependencyException : InvalidOperationException
{
  /// <summary>
  ///   Constructs a new instance of <see cref="MissingDependencyException" />.
  /// </summary>
  /// <param name="message">The exception message.</param>
  public MissingDependencyException(string message)
    : base(message)
  {
  }

  /// <summary>
  ///   Constructs a new instance of <see cref="MissingDependencyException" />.
  /// </summary>
  /// <param name="message">The exception message.</param>
  /// <param name="innerException">The inner/triggering exception.</param>
  public MissingDependencyException(string message, Exception innerException)
    : base(message, innerException)
  {
  }

  /// <summary>
  ///   Creates a <see cref="MissingDependencyException" /> tailored for a specific missing dependency type.
  /// </summary>
  /// <typeparam name="T">The type of the missing dependency.</typeparam>
  /// <returns>A new instance of <see cref="MissingDependencyException" /> tailored for the specified type.</returns>
  public static MissingDependencyException For<T>()
  {
    return new MissingDependencyException(
      $"Missing required dependency: {typeof(T).Name}. Verify dependency injection configuration."
    );
  }

  /// <summary>
  ///   Ensures that a specified value is not null. Throws a <see cref="MissingDependencyException" /> if the value is null.
  /// </summary>
  /// <typeparam name="T">The type of the object to check. Must be a reference type.</typeparam>
  /// <param name="value">The value to check for null.</param>
  /// <returns>The input value if it is not null.</returns>
  /// <exception cref="MissingDependencyException">
  ///   Thrown when the input value is null, indicating a missing required dependency.
  /// </exception>
  public static T ThrowIfNull<T>(T? value) where T : class
  {
    if (value == null)
    {
      throw For<T>();
    }

    return value;
  }
}
