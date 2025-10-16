using System.Text.Json;

namespace Jds.NiceNotice;

/// <summary>
///   An implementation of <see cref="NoticeSerializer" /> that uses JSON serialization.
/// </summary>
/// <param name="options">
///   Optional. JSON serialization options.
///   Defaults to <see cref="JsonDefaults.DefaultJsonSerializerOptions" />.
/// </param>
internal class JsonNoticeSerializer(JsonSerializerOptions? options = null) : NoticeSerializer
{
  /// <inheritdoc />
  public override string Serialize<TEventType>(TEventType notice)
  {
    // Must use the type of the notice to serialize it, not the type of the generic parameter.
    // Otherwise, only properties in the generic parameter will be serialized. (I.e., only the base type's properties.)
    // The generic method argument is a type filter.
    Type noticeType = notice.GetType();
    string json = JsonSerializer.Serialize(notice, noticeType, options ?? JsonDefaults.DefaultJsonSerializerOptions);

    return json;
  }
}

/// <summary>
///   An implementation of <see cref="NoticeSerializer{TEnterpriseEventBaseType}" /> that uses JSON serialization.
/// </summary>
/// <param name="options">
///   Optional. JSON serialization options.
///   Defaults to <see cref="JsonDefaults.DefaultJsonSerializerOptions" />.
/// </param>
/// <typeparam name="TEnterpriseEventBaseType">A notification base type.</typeparam>
internal class JsonNoticeSerializer<TEnterpriseEventBaseType>(JsonSerializerOptions? options = null)
  : NoticeSerializer<TEnterpriseEventBaseType>
{
  /// <inheritdoc />
  public override string Serialize<TEventType>(TEventType notice)
  {
    // Must use the type of the notice to serialize it, not the type of the generic parameter.
    // Otherwise, only properties in the generic parameter will be serialized. (I.e., only the base type's properties.)
    // The generic method argument is a type filter.
    Type noticeType = notice.GetType();
    string json = JsonSerializer.Serialize(notice, noticeType, options ?? JsonDefaults.DefaultJsonSerializerOptions);

    return json;
  }
}
