using System.Text.Json;

namespace Jds.NiceNotice.Tests.Unit;

internal static class TypedNoticeDispatchResultAssertionHelpers
{
  public static TEvent DeserializeIoResponseAsJson<TEvent>(
    this TypedNoticeDispatchResult<TEvent> result,
    JsonSerializerOptions? jsonSerializerOptions = null)
    where TEvent : notnull
  {
    return DeserializeIoResponseAsJson<TEvent>(result.IoResponse);
  }

  public static TEvent DeserializeIoResponseAsJson<TEvent>(
    string ioResponse,
    JsonSerializerOptions? jsonSerializerOptions = null)
    where TEvent : notnull
  {
    return JsonSerializer.Deserialize<TEvent>(
             ioResponse,
             jsonSerializerOptions ?? JsonDefaults.DefaultJsonSerializerOptions
           )
           ?? throw new InvalidOperationException(message: "Received null from deserialization.");
  }
}
