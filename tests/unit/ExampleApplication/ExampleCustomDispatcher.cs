using Xunit.Abstractions;

namespace Jds.NiceNotice.Tests.Unit.ExampleApplication;

/// <summary>
///   An implementation of <see cref="INoticeIo" /> which sends its notices to the xUnit <see cref="ITestOutputHelper" />.
/// </summary>
/// <remarks>
///   This example shows how an application implements a custom notice dispatch I/O.
///   This example is intentionally very simple, to focus on the interface.
/// </remarks>
/// <param name="testOutputHelper"></param>
public class ExampleCustomDispatcher(ITestOutputHelper testOutputHelper) : INoticeIo
{
  public Task<string> DispatchAsync(EventStreamId stream, string notice, CancellationToken cancellationToken = default)
  {
    // In this example we're writing to xUnit's test output.
    // In a real application you'd be sending this to a cloud/enterprise message bus, or maybe a logging-type service. 
    testOutputHelper.WriteLine(notice);

    return Task.FromResult(notice);
  }
}
