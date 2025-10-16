using System.Text.Json;

namespace Jds.NiceNotice.Tests.Unit;

internal static class TypedNoticeDispatchResultAssertionHelpers
{
  public static TEvent DeserializeIoResponseAsJson<TEvent>(
    this TypedNoticeDispatchResult<TEvent> result,
    JsonSerializerOptions? jsonSerializerOptions = null)
    where TEvent : notnull
  {
    return JsonSerializer.Deserialize<TEvent>(
             result.IoResponse,
             jsonSerializerOptions ?? JsonDefaults.DefaultJsonSerializerOptions
           )
           ?? throw new InvalidOperationException(message: "Received null from deserialization.");
  }
}
