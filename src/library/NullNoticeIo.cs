namespace Jds.NiceNotice;

/// <summary>
///     Provides a no-operation implementation of <see cref="INoticeIo" />,
///     primarily used as a default or placeholder where event dispatching is not required.
/// </summary>
public class NullNoticeIo : INoticeIo
{
    /// <summary>
    ///     Dispatches an asynchronous event to the specified event stream with the given notice.
    /// </summary>
    /// <remarks>
    ///     This implementation provides a no-operation mechanism, returning the given notice without processing.
    /// </remarks>
    /// <param name="stream">The event stream ID where the notice will be dispatched.</param>
    /// <param name="notice">The content of the notice to be dispatched.</param>
    /// <param name="cancellationToken">An asynchronous operation cancellation token.</param>
    /// <returns>
    ///     A task representing the asynchronous operation, containing the dispatched notice as its result.
    /// </returns>
    public Task<string> DispatchAsync(EventStreamId stream, string notice,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(notice);
    }
}
