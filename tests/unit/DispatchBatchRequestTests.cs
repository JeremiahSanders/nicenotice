using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Custom;
using Jds.NiceNotice.TypedNotices;
using Jds.TestingUtils.Randomization;

using Shouldly;

namespace Jds.NiceNotice.Tests.Unit;

public class DispatchBatchRequestTests
{
  public static DispatchBatchRequest<ExampleCustomBaseEnterpriseEvent> CreateWithCustomFunction(
    IEnumerable<ExampleCustomBaseEnterpriseEvent> notices
  )
  {
    return DispatchBatchRequest<ExampleCustomBaseEnterpriseEvent>.CreateFromTypedNotices(
      notices,
      static customEvent => Randomizer.Shared.RandomStringLatin(length: 64, alphanumeric: true),
      options: null
    );
  }

  public static DispatchBatchRequest<ExampleCustomBaseEnterpriseEvent> CreateWithDefaultFunction(
    IEnumerable<ExampleCustomBaseEnterpriseEvent> notices
  )
  {
    return DispatchBatchRequest<ExampleCustomBaseEnterpriseEvent>.CreateFromTypedNotices(notices, options: null);
  }

  [Fact]
  public void WithCustomIdFunction_CreatesExpectedResult()
  {
    List<ExampleCustomBaseEnterpriseEvent> notices =
    [
      new ExampleCustomLoginEvent
      {
        Username = "1"
      },
      new ExampleCustomLoginEvent
      {
        Username = "2"
      },
      new ExampleCustomLoginEvent
      {
        Username = "3"
      }
    ];

    DispatchBatchRequest<ExampleCustomBaseEnterpriseEvent> actual = CreateWithCustomFunction(notices);

    // We have all these notices.
    notices.ShouldAllBe(expected => actual.Notices.Values.Contains(expected));
  }

  [Fact]
  public void WithDefaultIdFunction_CreatesExpectedResult()
  {
    List<ExampleCustomBaseEnterpriseEvent> notices =
    [
      new ExampleCustomLoginEvent
      {
        Username = "1"
      },
      new ExampleCustomLoginEvent
      {
        Username = "2"
      },
      new ExampleCustomLoginEvent
      {
        Username = "3"
      }
    ];

    DispatchBatchRequest<ExampleCustomBaseEnterpriseEvent> actual = CreateWithDefaultFunction(notices);

    // We have all the notices.
    notices.ShouldAllBe(expected => actual.Notices.Values.Contains(expected));
    // Default id format should be GUID.
    actual
      .Notices.Keys.Select(key => new
        {
          key,
          isGuid = Guid.TryParse(key, out Guid _)
        }
      )
      .ShouldAllBe(tuple => tuple.isGuid);
  }
}
