using Jds.NiceNotice.Tests.Unit.ExampleEventSchemas.Standard;
using Jds.NiceNotice.TypedNotices;
using Jds.NiceNotice.TypedNotices.Validation;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

using Xunit.Abstractions;

namespace Jds.NiceNotice.Tests.Unit;

public class DataAnnotationsValidatorTests(ITestOutputHelper testOutputHelper)
{
  [Fact]
  public async Task UsingGenericDispatcher_InvokesDelegate()
  {
    Exception? exception = null;

    ITypedNoticeDispatcher<EnterpriseEvent> dispatcher = new ServiceCollection()
      .AddNiceNotice(builder => builder.UseTypedNotices(
          typedNoticesBuilder => { typedNoticesBuilder.ValidateWithDataAnnotations(); },
          ServiceLifetime.Singleton
        )
      )
      .BuildServiceProvider()
      .GetRequiredService<ITypedNoticeDispatcher<EnterpriseEvent>>();

    // Act
    TypedNoticeDispatchResult<ExampleLogoutEnterpriseEvent>? result = await dispatcher.TryDispatchAsync(
      new ExampleLogoutEnterpriseEvent
      {
        Username = string.Empty
      },
      ExceptionHandler
    );

    // Assert
    exception.ShouldNotBeNull();
    NoticeValidationException validatorException = exception
      .ShouldBeOfType<NoticeValidationException>();
    validatorException.ValidationFailures.ShouldNotBeEmpty();

    return;

    void ExceptionHandler(ExampleLogoutEnterpriseEvent arg1, Exception arg2)
    {
      exception = arg2;
      testOutputHelper.WriteLine($"Caught exception: {arg2}");
    }
  }

  [Fact]
  public async Task UsingUntypedDispatcher_InvokesDelegate()
  {
    Exception? exception = null;

    ITypedNoticeDispatcher dispatcher = new ServiceCollection()
      .AddNiceNotice(builder => builder.UseTypedNotices(
          typedNoticesBuilder => { typedNoticesBuilder.ValidateWithDataAnnotations(); },
          ServiceLifetime.Singleton
        )
      )
      .BuildServiceProvider()
      .GetRequiredService<ITypedNoticeDispatcher>();

    // Act
    TypedNoticeDispatchResult<ExampleLogoutEnterpriseEvent>? result = await dispatcher.TryDispatchAsync(
      new ExampleLogoutEnterpriseEvent
      {
        Username = string.Empty
      },
      ExceptionHandler
    );

    // Assert
    exception.ShouldNotBeNull();
    NoticeValidationException validatorException = exception
      .ShouldBeOfType<NoticeValidationException>();
    validatorException.ValidationFailures.ShouldNotBeEmpty();

    return;

    void ExceptionHandler(ExampleLogoutEnterpriseEvent arg1, Exception arg2)
    {
      exception = arg2;
      testOutputHelper.WriteLine($"Caught exception: {arg2}");
    }
  }
}
