using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jds.NiceNotice;

/// <summary>
///   Provides default JSON serialization settings for the library.
/// </summary>
public static class JsonDefaults
{
  /// <summary>
  ///   Gets the default JSON serializer options to be used for notices.
  ///   This includes pre-configured settings and converters, such as a <see cref="JsonStringEnumConverter" /> for
  ///   serializing enums as strings.
  /// </summary>
  public static JsonSerializerOptions DefaultJsonSerializerOptions { get; } = new(JsonSerializerDefaults.Web)
  {
    Converters =
    {
      new JsonStringEnumConverter()
    }
  };
}
