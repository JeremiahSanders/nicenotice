using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Jds.NiceNotice.Tests.Unit;

/// <summary>
///   These tests verify that the custom delegate-based notice validator is invoked
///   when using the generic or untyped dispatcher.
/// </summary>
public class DelegateNoticeValidatorTests
{
  [Fact]
  public async Task UsingUntypedDispatcher_InvokesDelegate()
  {
    int genericInvocations = 0;
    int untypedInvocations = 0;

    ITypedNoticeDispatcher dispatcher = new ServiceCollection()
      .AddNiceNotice(builder => builder.UseTypedNotices(
          typedNoticesBuilder =>
          {
            typedNoticesBuilder.WithDelegateValidator(GenericValidatorReference, UntypedValidatorReference);
          },
          ServiceLifetime.Singleton
        )
      )
      .BuildServiceProvider()
      .GetRequiredService<ITypedNoticeDispatcher>();

    // Act
    TypedNoticeDispatchResult<EnterpriseEventMessage> result = await dispatcher.DispatchAsync(
      new EnterpriseEventMessage
      {
        Message = Guid
          .NewGuid()
          .ToString()
      }
    );

    // Assert
    result.ShouldNotBeNull();
    genericInvocations.ShouldBe(expected: 0);
    untypedInvocations.ShouldBe(expected: 1);

    return;

    IReadOnlyList<string>? UntypedValidatorReference(object notice, string serialized)
    {
      untypedInvocations++;

      return null;
    }

    IReadOnlyList<string>? GenericValidatorReference(EnterpriseEventBase notice, string serialized)
    {
      genericInvocations++;

      return null;
    }
  }

  [Fact]
  public async Task UsingGenericDispatcher_InvokesDelegate()
  {
    int genericInvocations = 0;
    int untypedInvocations = 0;

    ITypedNoticeDispatcher<EnterpriseEventBase> dispatcher = new ServiceCollection()
      .AddNiceNotice(builder => builder.UseTypedNotices(
          typedNoticesBuilder =>
          {
            typedNoticesBuilder.WithDelegateValidator(GenericValidatorReference, UntypedValidatorReference);
          },
          ServiceLifetime.Singleton
        )
      )
      .BuildServiceProvider()
      .GetRequiredService<ITypedNoticeDispatcher<EnterpriseEventBase>>();

    // Act
    TypedNoticeDispatchResult<EnterpriseEventMessage> result = await dispatcher.DispatchAsync(
      new EnterpriseEventMessage
      {
        Message = Guid
          .NewGuid()
          .ToString()
      }
    );

    // Assert
    genericInvocations.ShouldBe(expected: 1);
    untypedInvocations.ShouldBe(expected: 0);

    return;

    IReadOnlyList<string>? UntypedValidatorReference(object notice, string serialized)
    {
      untypedInvocations++;

      return null;
    }

    IReadOnlyList<string>? GenericValidatorReference(EnterpriseEventBase notice, string serialized)
    {
      genericInvocations++;

      return null;
    }
  }
}
