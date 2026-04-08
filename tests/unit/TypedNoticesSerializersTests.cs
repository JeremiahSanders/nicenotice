using System.Text.Json;

using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Standard;
using Jds.NiceNotice.TypedNotices.Serialization;
using Jds.TestingUtils.Randomization;

using Shouldly;

namespace Jds.NiceNotice.Tests.Unit;

public class TypedNoticesSerializersTests
{
  [Fact]
  public void Json_Generic_SerializesUsingDefaultOptions()
  {
    JsonSerializerOptions defaultOptions = JsonDefaults.DefaultJsonSerializerOptions;
    JsonSerializerOptions? nullOptions = null;
    NoticeSerializer<EnterpriseEvent> withDefaultOptions = Serializers.Json<EnterpriseEvent>(defaultOptions);
    NoticeSerializer<EnterpriseEvent> withNullOptions = Serializers.Json<EnterpriseEvent>(nullOptions);
    ExampleLoginEnterpriseEvent notice = new()
    {
      Username = Randomizer.Shared.DemographicsForenameUsa()
    };

    string serializedUsingDefaultOptions = withDefaultOptions.Serialize(notice);
    string serializedUsingNullOptions = withNullOptions.Serialize(notice);

    serializedUsingDefaultOptions.ShouldNotBeNullOrWhiteSpace();
    serializedUsingNullOptions.ShouldNotBeNullOrWhiteSpace();
    serializedUsingNullOptions.ShouldBe(serializedUsingDefaultOptions);
  }

  [Fact]
  public void Json_NonGeneric_GeneratesSerializer()
  {
    JsonSerializerOptions defaultOptions = JsonDefaults.DefaultJsonSerializerOptions;
    JsonSerializerOptions? nullOptions = null;
    NoticeSerializer withDefaultOptions = Serializers.Json(defaultOptions);
    NoticeSerializer withNullOptions = Serializers.Json(nullOptions);
    ExampleLoginEnterpriseEvent notice = new()
    {
      Username = Randomizer.Shared.DemographicsForenameUsa()
    };

    string serializedUsingDefaultOptions = withDefaultOptions.Serialize(notice);
    string serializedUsingNullOptions = withNullOptions.Serialize(notice);

    serializedUsingDefaultOptions.ShouldNotBeNullOrWhiteSpace();
    serializedUsingNullOptions.ShouldNotBeNullOrWhiteSpace();
    serializedUsingNullOptions.ShouldBe(serializedUsingDefaultOptions);
  }
}
