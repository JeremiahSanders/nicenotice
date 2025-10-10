namespace Jds.NiceNotice;

/// <summary>
///   An abstraction representing the algorithm used for serializing a notice to a string.
/// </summary>
public abstract class NoticeSerializer
{
  /// <summary>
  ///   Serialize the given notice to a string.
  /// </summary>
  /// <param name="notice">A notice to serialize.</param>
  /// <typeparam name="TEventType">A notice object type.</typeparam>
  /// <returns>Returns the serialized notice.</returns>
  public abstract string Serialize<TEventType>(TEventType notice) where TEventType : notnull;
}

/// <summary>
///   An abstraction representing the algorithm used for serializing a notice to a string.
/// </summary>
/// <typeparam name="TEnterpriseEventBaseType">A base notification object type.</typeparam>
public abstract class NoticeSerializer<TEnterpriseEventBaseType>
{
  /// <summary>
  ///   Serializes the given notice to a string.
  ///   Values are restricted to subtypes of <typeparamref name="TEnterpriseEventBaseType" />.
  /// </summary>
  /// <param name="notice">A notice to serialize.</param>
  /// <typeparam name="TEventType">
  ///   A notice type, which must be a subtype of <typeparamref name="TEnterpriseEventBaseType" />.
  /// </typeparam>
  /// <returns>Returns the serialized notice.</returns>
  public abstract string Serialize<TEventType>(TEventType notice)
    where TEventType : notnull, TEnterpriseEventBaseType;
}
