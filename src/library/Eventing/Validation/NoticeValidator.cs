namespace Jds.NiceNotice;

/// <summary>
///   An abstraction representing the algorithm used for validating a notice.
/// </summary>
public abstract class NoticeValidator<TEnterpriseEventBaseType>
  where TEnterpriseEventBaseType : notnull
{
  /// <summary>
  ///   Performs validation on the given notice, which is a subtype of <typeparamref name="TEnterpriseEventBaseType" />.
  ///   Returns null if the notice is valid, otherwise a list of validation errors.
  /// </summary>
  /// <param name="notice">The notice object being validated.</param>
  /// <param name="serializedNotice">
  ///   The serialized representation of <paramref name="notice" />.
  ///   This is useful for applying length constraints
  ///   (i.e., to verify it won't exceed documented limits of your I/O destination).
  /// </param>
  /// <typeparam name="TEventType">The notice object type.</typeparam>
  /// <returns>
  ///   Returns null if the notice is valid, otherwise a list of validation errors.
  /// </returns>
  public abstract IReadOnlyList<string>? Validate<TEventType>(TEventType notice, string serializedNotice)
    where TEventType : TEnterpriseEventBaseType;
}

/// <summary>
///   An abstraction representing the algorithm used for validating a notice.
/// </summary>
public abstract class NoticeValidator
{
  /// <summary>
  ///   Performs validation on the given notice.
  ///   Returns null if the notice is valid, otherwise a list of validation errors.
  /// </summary>
  /// <param name="notice">The notice object being validated.</param>
  /// <param name="serializedNotice">
  ///   The serialized representation of <paramref name="notice" />.
  ///   This is useful for applying length constraints
  ///   (i.e., to verify it won't exceed documented limits of your I/O destination).
  /// </param>
  /// <typeparam name="TEventType">The notice object type.</typeparam>
  /// <returns>
  ///   Returns null if the notice is valid, otherwise a list of validation errors.
  /// </returns>
  public abstract IReadOnlyList<string>? Validate<TEventType>(TEventType notice, string serializedNotice)
    where TEventType : notnull;
}
