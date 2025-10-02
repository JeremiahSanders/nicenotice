using System.Text.Json;

namespace Jds.NiceNotice;

internal class JsonNoticeSerializer : NoticeSerializer
{
  private readonly JsonSerializerOptions? _options;

  public JsonNoticeSerializer(JsonSerializerOptions? options = null)
  {
    _options = options;
  }


  public override string Serialize<TEventType>(TEventType notice)
  {
    // Must use the type of the notice to serialize it, not the type of the generic parameter.
    // Otherwise, only properties in the generic parameter will be serialized. (I.e., only the base type's properties.)
    // The generic method argument is a type filter.
    Type noticeType = notice.GetType();
    string json = JsonSerializer.Serialize(notice, noticeType, _options ?? JsonDefaults.DefaultJsonSerializerOptions);

    return json;
  }
}

internal class JsonNoticeSerializer<TEnterpriseEventBaseType> : NoticeSerializer<TEnterpriseEventBaseType>
{
  private readonly JsonSerializerOptions? _options;

  public JsonNoticeSerializer(JsonSerializerOptions? options = null)
  {
    _options = options;
  }


  public override string Serialize<TEventType>(TEventType notice)
  {
    // Must use the type of the notice to serialize it, not the type of the generic parameter.
    // Otherwise, only properties in the generic parameter will be serialized. (I.e., only the base type's properties.)
    // The generic method argument is a type filter.
    Type noticeType = notice.GetType();
    string json = JsonSerializer.Serialize(notice, noticeType, _options ?? JsonDefaults.DefaultJsonSerializerOptions);

    return json;
  }
}
