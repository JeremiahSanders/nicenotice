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
  ///   This uses the <see cref="JsonSerializerDefaults.Web" /> settings as a base,
  ///   adds  <see cref="JsonStringEnumConverter" /> for serializing enums as strings,
  ///   and sets <see cref="JsonSerializerOptions.DefaultIgnoreCondition" /> to
  ///   <see cref="JsonIgnoreCondition.WhenWritingNull" />.
  /// </summary>
  public static JsonSerializerOptions DefaultJsonSerializerOptions { get; } = new(JsonSerializerDefaults.Web)
  {
    Converters =
    {
      new JsonStringEnumConverter()
    },
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
  };
}
