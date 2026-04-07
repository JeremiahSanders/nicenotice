namespace Jds.NiceNotice;

/// <summary>
///   A notice metadata value.
/// </summary>
/// <remarks>
///   This type wraps multiple primitives to provide a unified metadata value type
///   (while more constrained than <see cref="object" />).
/// </remarks>
public record NoticeMetadataValue
{
  /// <summary>
  ///   Constructs a new instance of <see cref="NoticeMetadataValue" /> with an integer value.
  /// </summary>
  /// <param name="value">An integer value.</param>
  public NoticeMetadataValue(int value)
  {
    IntValue = value;
    StringValue = null;
    DoubleValue = null;
  }

  /// <summary>
  ///   Constructs a new instance of <see cref="NoticeMetadataValue" /> with a string value.
  /// </summary>
  /// <param name="value">A string value.</param>
  public NoticeMetadataValue(string value)
  {
    IntValue = null;
    StringValue = value;
    DoubleValue = null;
  }

  /// <summary>
  ///   Constructs a new instance of <see cref="NoticeMetadataValue" /> with a double value.
  /// </summary>
  /// <param name="value">A double value.</param>
  public NoticeMetadataValue(double value)
  {
    IntValue = null;
    StringValue = null;
    DoubleValue = value;
  }

  /// <summary>
  ///   Gets the double value.
  /// </summary>
  public double? DoubleValue { get; }

  /// <summary>
  ///   Gets the integer value.
  /// </summary>
  public int? IntValue { get; }

  /// <summary>
  ///   Gets a value indicating whether the value is a double.
  /// </summary>
  public bool IsDouble => DoubleValue != null;

  /// <summary>
  ///   Gets a value indicating whether the value is an integer.
  /// </summary>
  public bool IsInt => IntValue != null;

  /// <summary>
  ///   Gets a value indicating whether the value is a string.
  /// </summary>
  public bool IsString => StringValue != null;

  /// <summary>
  ///   Gets the string value.
  /// </summary>
  public string? StringValue { get; }

  /// <summary>
  ///   Creates a new <see cref="NoticeMetadataValue" /> from an integer.
  /// </summary>
  /// <param name="value">This integer value.</param>
  /// <returns>Returns a <see cref="NoticeMetadataValue" /> instance.</returns>
  public static implicit operator NoticeMetadataValue(int value)
  {
    return new NoticeMetadataValue(value);
  }

  /// <summary>
  ///   Creates a new <see cref="NoticeMetadataValue" /> from a double.
  /// </summary>
  /// <param name="value">This double value.</param>
  /// <returns>Returns a <see cref="NoticeMetadataValue" /> instance.</returns>
  public static implicit operator NoticeMetadataValue(double value)
  {
    return new NoticeMetadataValue(value);
  }

  /// <summary>
  ///   Creates a new <see cref="NoticeMetadataValue" /> from a string.
  /// </summary>
  /// <param name="value">This string value.</param>
  /// <returns>Returns a <see cref="NoticeMetadataValue" /> instance.</returns>
  public static implicit operator NoticeMetadataValue(string value)
  {
    return new NoticeMetadataValue(value);
  }

  /// <inheritdoc />
  /// <remarks>Returns the string value or converts the value to string.</remarks>
  public override string ToString()
  {
    return StringValue ?? IntValue?.ToString() ?? DoubleValue?.ToString() ?? string.Empty;
  }
}
