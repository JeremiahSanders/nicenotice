using System.Diagnostics.CodeAnalysis;

using Jds.NiceNotice.Configuration;
using Jds.NiceNotice.TypedNotices;
using Jds.NiceNotice.TypedNotices.Routing;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Jds.NiceNotice.Tests.Unit;

/// <summary>
///   Tests verifying whether the <see cref="NoticeStreamAttribute" /> is properly considered when routing typed notices.
/// </summary>
[SuppressMessage(category: "Usage", checkId: "xUnit1026:Theory methods should use all of their parameters")]
public class NoticeStreamAttributeTests
{
  public static IEnumerable<object?[]> CreateBaseTypeNoAttributeTestCases()
  {
    // Overridden at notice level
    yield return Create(
      TypedNoticesBuilderOptions.RoutingTypes.TypeFullName,
      new NoticeStreamEvents.NoAttribute.DerivedL2NoAttributeNoAttributeWithAttribute(),
      NoticeStreamEvents.DerivedL2StreamName
    );
    yield return Create(
      TypedNoticesBuilderOptions.RoutingTypes.TypeName,
      new NoticeStreamEvents.NoAttribute.DerivedL2NoAttributeNoAttributeWithAttribute(),
      NoticeStreamEvents.DerivedL2StreamName
    );

    // No route attribute in hierarchy
    yield return Create(
      TypedNoticesBuilderOptions.RoutingTypes.TypeFullName,
      new NoticeStreamEvents.NoAttribute.DerivedL1NoAttributeNoAttribute(),
      typeof(NoticeStreamEvents.NoAttribute.DerivedL1NoAttributeNoAttribute).FullName.ShouldNotBeNull()
    );
    yield return Create(
      TypedNoticesBuilderOptions.RoutingTypes.TypeName,
      new NoticeStreamEvents.NoAttribute.DerivedL1NoAttributeNoAttribute(),
      nameof(NoticeStreamEvents.NoAttribute.DerivedL1NoAttributeNoAttribute)
    );

    // Overridden at parent notice level, not specified at notice level
    yield return Create(
      TypedNoticesBuilderOptions.RoutingTypes.TypeFullName,
      new NoticeStreamEvents.NoAttribute.DerivedL2NoAttributeWithAttributeNoAttribute(),
      NoticeStreamEvents.DerivedL1StreamName
    );
    yield return Create(
      TypedNoticesBuilderOptions.RoutingTypes.TypeName,
      new NoticeStreamEvents.NoAttribute.DerivedL2NoAttributeWithAttributeNoAttribute(),
      NoticeStreamEvents.DerivedL1StreamName
    );

    yield break;

    static object?[] Create(
      TypedNoticesBuilderOptions.RoutingTypes routingType,
      NoticeStreamEvents.NoAttribute.BaseTypeNoAttribute notice,
      string expected
    )
    {
      return new object?[]
      {
        $"{routingType} - {notice.GetType().Name}",
        routingType,
        notice.GetType().FullName.ShouldNotBeNull(),
        expected
      };
    }
  }

  public static IEnumerable<object?[]> CreateBaseTypeWithAttributeTestCases()
  {
    // Overridden at notice level
    yield return Create(
      TypedNoticesBuilderOptions.RoutingTypes.TypeFullName,
      new NoticeStreamEvents.WithAttribute.DerivedL1WithAttributeWithAttribute(),
      NoticeStreamEvents.DerivedL1StreamName
    );
    yield return Create(
      TypedNoticesBuilderOptions.RoutingTypes.TypeName,
      new NoticeStreamEvents.WithAttribute.DerivedL1WithAttributeWithAttribute(),
      NoticeStreamEvents.DerivedL1StreamName
    );
    yield return Create(
      TypedNoticesBuilderOptions.RoutingTypes.TypeFullName,
      new NoticeStreamEvents.WithAttribute.BaseTypeWithAttribute(),
      NoticeStreamEvents.BaseTypeStreamName
    );
    yield return Create(
      TypedNoticesBuilderOptions.RoutingTypes.TypeName,
      new NoticeStreamEvents.WithAttribute.BaseTypeWithAttribute(),
      NoticeStreamEvents.BaseTypeStreamName
    );

    // Overridden at parent notice level, not specified at notice level
    yield return Create(
      TypedNoticesBuilderOptions.RoutingTypes.TypeFullName,
      new NoticeStreamEvents.WithAttribute.DerivedL2WithAttributeNoAttribute(),
      NoticeStreamEvents.DerivedL1StreamName
    );
    yield return Create(
      TypedNoticesBuilderOptions.RoutingTypes.TypeName,
      new NoticeStreamEvents.WithAttribute.DerivedL2WithAttributeNoAttribute(),
      NoticeStreamEvents.DerivedL1StreamName
    );

    // overriden at grandparent notice level, not specified at notice level
    yield return Create(
      TypedNoticesBuilderOptions.RoutingTypes.TypeFullName,
      new NoticeStreamEvents.WithAttribute.DerivedL2WithAttributeNoAttributeNoAttribute(),
      NoticeStreamEvents.BaseTypeStreamName
    );
    yield return Create(
      TypedNoticesBuilderOptions.RoutingTypes.TypeName,
      new NoticeStreamEvents.WithAttribute.DerivedL2WithAttributeNoAttributeNoAttribute(),
      NoticeStreamEvents.BaseTypeStreamName
    );

    yield break;

    static object?[] Create(
      TypedNoticesBuilderOptions.RoutingTypes routingType,
      NoticeStreamEvents.WithAttribute.BaseTypeWithAttribute notice,
      string expected
    )
    {
      return new object?[]
      {
        $"{routingType} - {notice.GetType().Name}",
        routingType,
        notice.GetType().FullName.ShouldNotBeNull(),
        expected
      };
    }
  }

  private static NoticeStreamEvents.NoAttribute.BaseTypeNoAttribute CreateNoAttributeNoticeByName(string noticeType)
  {
    return (NoticeStreamEvents.NoAttribute.BaseTypeNoAttribute)Activator
      .CreateInstance(Type.GetType(noticeType).ShouldNotBeNull())
      .ShouldNotBeNull();
  }

  private static NoticeStreamEvents.WithAttribute.BaseTypeWithAttribute CreateWithAttributeNoticeByName(
    string noticeType)
  {
    return (NoticeStreamEvents.WithAttribute.BaseTypeWithAttribute)Activator
      .CreateInstance(Type.GetType(noticeType).ShouldNotBeNull())
      .ShouldNotBeNull();
  }

  public static class NoticeStreamEvents
  {
    public const string BaseTypeStreamName = "base-type-stream";
    public const string DerivedL1StreamName = "derived-l1-stream";
    public const string DerivedL2StreamName = "derived-l2-stream";

    public static class NoAttribute
    {
      /// <summary>
      ///   A base notice type with no routing attribute.
      /// </summary>
      public class BaseTypeNoAttribute
      {
        public virtual string? ExpectedStreamName => null;
        public string Id { get; init; } = Guid.NewGuid().ToString();
      }

      /// <summary>
      ///   A first-level derived notice which has an attribute from a base which lacks an attribute.
      /// </summary>
      [NoticeStream(DerivedL1StreamName)]
      public class DerivedL1NoAttributeWithAttribute : BaseTypeNoAttribute
      {
        public override string? ExpectedStreamName => DerivedL1StreamName;
      }

      /// <summary>
      ///   A second-level derived notice which lacks an attribute
      ///   from a level-one base which has an attribute
      ///   from a root base which lacks an attribute.
      /// </summary>
      public class DerivedL2NoAttributeWithAttributeNoAttribute : DerivedL1NoAttributeWithAttribute
      {
      }

      /// <summary>
      ///   A first-level derived notice which lacks an attribute from a base which lacks an attribute.
      /// </summary>
      public class DerivedL1NoAttributeNoAttribute : BaseTypeNoAttribute
      {
      }

      [NoticeStream(DerivedL2StreamName)]
      public class DerivedL2NoAttributeNoAttributeWithAttribute : DerivedL1NoAttributeNoAttribute
      {
      }
    }

    public static class WithAttribute
    {
      [NoticeStream(BaseTypeStreamName)]
      public class BaseTypeWithAttribute
      {
        public virtual string? ExpectedStreamName => BaseTypeStreamName;
        public string Id { get; init; } = Guid.NewGuid().ToString();
      }

      [NoticeStream(DerivedL1StreamName)]
      public class DerivedL1WithAttributeWithAttribute : BaseTypeWithAttribute
      {
        public override string? ExpectedStreamName => DerivedL1StreamName;
      }

      public class DerivedL2WithAttributeNoAttribute : DerivedL1WithAttributeWithAttribute
      {
      }

      public class DerivedL1WithAttributeNoAttribute : BaseTypeWithAttribute
      {
      }

      public class DerivedL2WithAttributeNoAttributeNoAttribute : DerivedL1WithAttributeNoAttribute
      {
      }
    }
  }

  public class DefaultTypedNoticeDispatcherTypedTests : NoticeStreamAttributeTests
  {
    public static ITypedNoticeDispatcher<TEventType> Create<TEventType>(
      TypedNoticesBuilderOptions.RoutingTypes routingType
    )
      where TEventType : notnull
    {
      ServiceProvider provider = new ServiceCollection()
        .AddNiceNotice(nnb =>
          nnb.UseTypedNotices<TEventType>(
            tnb =>
            {
              // Use default config (which we assume routes to type names), unless full is specified by the case
              if (routingType == TypedNoticesBuilderOptions.RoutingTypes.TypeFullName)
              {
                tnb.RouteToTypeNameStreams(routingType == TypedNoticesBuilderOptions.RoutingTypes.TypeFullName);
              }
            },
            ServiceLifetime.Transient
          )
        )
        .BuildServiceProvider();

      return provider.GetRequiredService<ITypedNoticeDispatcher<TEventType>>();
    }

    [Theory]
    [MemberData(nameof(CreateBaseTypeNoAttributeTestCases))]
    public async Task DispatchAsync_GivenBaseTypeNoAttribute_InfersExpectedStreamName(
      string caseName,
      TypedNoticesBuilderOptions.RoutingTypes routingType,
      string noticeType,
      string expected
    )
    {
      ITypedNoticeDispatcher<NoticeStreamEvents.NoAttribute.BaseTypeNoAttribute> dispatcher =
        Create<NoticeStreamEvents.NoAttribute.BaseTypeNoAttribute>(routingType);

      NoticeStreamEvents.NoAttribute.BaseTypeNoAttribute notice = CreateNoAttributeNoticeByName(noticeType);
      TypedNoticeDispatchResult<NoticeStreamEvents.NoAttribute.BaseTypeNoAttribute> result =
        await dispatcher.DispatchAsync(notice);

      result.IoRequest.Stream.ShouldBe(EventStreamId.From(expected));
    }

    [Theory]
    [MemberData(nameof(CreateBaseTypeWithAttributeTestCases))]
    public async Task DispatchAsync_GivenBaseTypeWithAttribute_InfersExpectedStreamName(
      string caseName,
      TypedNoticesBuilderOptions.RoutingTypes routingType,
      string noticeType,
      string expected
    )
    {
      ITypedNoticeDispatcher<NoticeStreamEvents.WithAttribute.BaseTypeWithAttribute> dispatcher =
        Create<NoticeStreamEvents.WithAttribute.BaseTypeWithAttribute>(routingType);

      NoticeStreamEvents.WithAttribute.BaseTypeWithAttribute notice = CreateWithAttributeNoticeByName(noticeType);
      TypedNoticeDispatchResult<NoticeStreamEvents.WithAttribute.BaseTypeWithAttribute> result =
        await dispatcher.DispatchAsync(notice);

      result.IoRequest.Stream.ShouldBe(EventStreamId.From(expected));
    }

    [Theory]
    [MemberData(nameof(CreateBaseTypeNoAttributeTestCases))]
    public async Task DispatchBatchAsync_GivenBaseTypeNoAttribute_InfersExpectedStreamName(
      string caseName,
      TypedNoticesBuilderOptions.RoutingTypes routingType,
      string noticeType,
      string expected
    )
    {
      ITypedNoticeDispatcher<NoticeStreamEvents.NoAttribute.BaseTypeNoAttribute> dispatcher =
        Create<NoticeStreamEvents.NoAttribute.BaseTypeNoAttribute>(routingType);

      NoticeStreamEvents.NoAttribute.BaseTypeNoAttribute notice = CreateNoAttributeNoticeByName(noticeType);
      BatchTypedNoticeDispatchResult result =
        await dispatcher.DispatchBatchAsync([notice]);

      result
        .Successes.ShouldHaveSingleItem()
        .Stream.ShouldBe(EventStreamId.From(expected));
    }

    [Theory]
    [MemberData(nameof(CreateBaseTypeWithAttributeTestCases))]
    public async Task DispatchBatchAsync_GivenBaseTypeWithAttribute_InfersExpectedStreamName(
      string caseName,
      TypedNoticesBuilderOptions.RoutingTypes routingType,
      string noticeType,
      string expected
    )
    {
      ITypedNoticeDispatcher<NoticeStreamEvents.WithAttribute.BaseTypeWithAttribute> dispatcher =
        Create<NoticeStreamEvents.WithAttribute.BaseTypeWithAttribute>(routingType);

      NoticeStreamEvents.WithAttribute.BaseTypeWithAttribute notice = CreateWithAttributeNoticeByName(noticeType);
      BatchTypedNoticeDispatchResult result =
        await dispatcher.DispatchBatchAsync([notice]);

      result
        .Successes.ShouldHaveSingleItem()
        .Stream.ShouldBe(EventStreamId.From(expected));
    }
  }

  public class DefaultTypedNoticeDispatcherUntypedTests : NoticeStreamAttributeTests
  {
    public static ITypedNoticeDispatcher Create(TypedNoticesBuilderOptions.RoutingTypes routingType)
    {
      ServiceProvider provider = new ServiceCollection()
        .AddNiceNotice(nnb =>
          nnb.UseTypedNotices(
            tnb =>
            {
              // Use default config (which we assume routes to type names), unless full is specified by the case
              if (routingType == TypedNoticesBuilderOptions.RoutingTypes.TypeFullName)
              {
                tnb.RouteToTypeNameStreams(routingType == TypedNoticesBuilderOptions.RoutingTypes.TypeFullName);
              }
            },
            ServiceLifetime.Transient
          )
        )
        .BuildServiceProvider();

      return provider.GetRequiredService<ITypedNoticeDispatcher>();
    }

    [Theory]
    [MemberData(nameof(CreateBaseTypeNoAttributeTestCases))]
    public async Task DispatchAsync_GivenBaseTypeNoAttribute_InfersExpectedStreamName(
      string caseName,
      TypedNoticesBuilderOptions.RoutingTypes routingType,
      string noticeType,
      string expected
    )
    {
      ITypedNoticeDispatcher dispatcher = Create(routingType);

      NoticeStreamEvents.NoAttribute.BaseTypeNoAttribute notice = CreateNoAttributeNoticeByName(noticeType);
      bool defaultToFullName = routingType == TypedNoticesBuilderOptions.RoutingTypes.TypeFullName;
      TypedNoticeDispatchResult<NoticeStreamEvents.NoAttribute.BaseTypeNoAttribute> result =
        await dispatcher.DispatchAsync(notice, defaultToFullName);

      result.IoRequest.Stream.ShouldBe(EventStreamId.From(expected));
    }

    [Theory]
    [MemberData(nameof(CreateBaseTypeWithAttributeTestCases))]
    public async Task DispatchAsync_GivenBaseTypeWithAttribute_InfersExpectedStreamName(
      string caseName,
      TypedNoticesBuilderOptions.RoutingTypes routingType,
      string noticeType,
      string expected
    )
    {
      ITypedNoticeDispatcher dispatcher = Create(routingType);

      NoticeStreamEvents.WithAttribute.BaseTypeWithAttribute notice = CreateWithAttributeNoticeByName(noticeType);
      bool defaultToFullName = routingType == TypedNoticesBuilderOptions.RoutingTypes.TypeFullName;
      TypedNoticeDispatchResult<NoticeStreamEvents.WithAttribute.BaseTypeWithAttribute> result =
        await dispatcher.DispatchAsync(notice, defaultToFullName);

      result.IoRequest.Stream.ShouldBe(EventStreamId.From(expected));
    }

    [Theory]
    [MemberData(nameof(CreateBaseTypeNoAttributeTestCases))]
    public async Task DispatchBatchAsync_GivenBaseTypeNoAttribute_InfersExpectedStreamName(
      string caseName,
      TypedNoticesBuilderOptions.RoutingTypes routingType,
      string noticeType,
      string expected
    )
    {
      ITypedNoticeDispatcher dispatcher = Create(routingType);

      NoticeStreamEvents.NoAttribute.BaseTypeNoAttribute notice = CreateNoAttributeNoticeByName(noticeType);
      bool defaultToFullName = routingType == TypedNoticesBuilderOptions.RoutingTypes.TypeFullName;
      BatchTypedNoticeDispatchResult result =
        await dispatcher.DispatchBatchToInferredRoutesAsync([notice], defaultToFullName);

      result
        .Successes.ShouldHaveSingleItem()
        .Stream.ShouldBe(EventStreamId.From(expected));
    }

    [Theory]
    [MemberData(nameof(CreateBaseTypeWithAttributeTestCases))]
    public async Task DispatchBatchAsync_GivenBaseTypeWithAttribute_InfersExpectedStreamName(
      string caseName,
      TypedNoticesBuilderOptions.RoutingTypes routingType,
      string noticeType,
      string expected
    )
    {
      ITypedNoticeDispatcher dispatcher = Create(routingType);

      NoticeStreamEvents.WithAttribute.BaseTypeWithAttribute notice = CreateWithAttributeNoticeByName(noticeType);
      bool defaultToFullName = routingType == TypedNoticesBuilderOptions.RoutingTypes.TypeFullName;
      BatchTypedNoticeDispatchResult result =
        await dispatcher.DispatchBatchToInferredRoutesAsync([notice], defaultToFullName);

      result
        .Successes.ShouldHaveSingleItem()
        .Stream.ShouldBe(EventStreamId.From(expected));
    }
  }
}
