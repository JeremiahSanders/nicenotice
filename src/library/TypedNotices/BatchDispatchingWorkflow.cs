using System.Collections.Concurrent;

using Jds.NiceNotice.Dispatching;

namespace Jds.NiceNotice.TypedNotices;

/// <summary>
///   Methods performing a batch dispatch workflow.
///   This abstraction provides consistent support for both <see cref="TypedNoticeDispatcher" />
///   and <see cref="TypedNoticeDispatcher{TEnterpriseEventBaseType}" />.
/// </summary>
internal static class BatchDispatchingWorkflow
{
  public static async Task<BatchTypedNoticeDispatchResult> DispatchBatchAsync(
    Func<INoticeIo> ioDispatcherProvider,
    Func<string, BatchRoutedTypedNoticeRequest, BatchRoutedTypedNoticeResponse> trySerializeAndValidate,
    IReadOnlyDictionary<string, BatchRoutedTypedNoticeRequest> notices,
    BatchDispatchOptions? batchDispatchOptions = null,
    CancellationToken cancellationToken = default)
  {
    ConcurrentBag<BatchRoutedTypedNoticeResponse> failures = [];
    ConcurrentBag<BatchRoutedTypedNoticeResponse> successes = [];


    List<BatchRoutedTypedNoticeResponse> serializedValidatedAndRouted = notices
      .Select(notice =>
        Eithers
          .Try(() => trySerializeAndValidate(notice.Key, notice.Value))
          .Match(
            exception => new BatchRoutedTypedNoticeResponse(
              notice.Key,
              notice.Value.Stream,
              notice.Value.Notice,
              string.Empty,
              contentType: null,
              metadata: null,
              exception
            ),
            static success => success
          )
      )
      .ToList();

    foreach (BatchRoutedTypedNoticeResponse failedNotice in serializedValidatedAndRouted.Where(r => !r.IsSuccessful))
    {
      failures.Add(failedNotice);
    }

    List<BatchRoutedTypedNoticeResponse> validatedAndRouted =
      serializedValidatedAndRouted.Where(r => r.IsSuccessful).ToList();

    if (validatedAndRouted.Count > 0)
    {
      if (ioDispatcherProvider() is INoticeBatchIo batchIo)
      {
        IoBatchNoticeDispatchResult batchIoResult = (await Eithers.TryAsync(async () =>
            await batchIo.DispatchNoticesAsync(
              new IoBatchNoticeDispatchRequest(
                validatedAndRouted
                  .ToDictionary(
                    static rtn => rtn.BatchNoticeId,
                    static rtn => new IoNoticeDispatchRequest(rtn.Stream, rtn.Notice, rtn.Metadata, rtn.ContentType)
                  ),
                batchDispatchOptions
              ),
              cancellationToken
            )
          ))
          .MapLeft(exception => new IoBatchNoticeDispatchResult(
              validatedAndRouted
                .Select(n =>
                  new IoBatchNoticeDispatchResultItem(n.BatchNoticeId, n.Stream, n.Notice, n.Metadata, n.ContentType, exception)
                )
            )
          )
          .Match(i => i, i => i);

        IEnumerable<BatchRoutedTypedNoticeResponse> mappedFailures =
          from bf in batchIoResult.Failures
          join requestKvp in notices
            on bf.BatchNoticeId equals requestKvp.Key
          select
            new BatchRoutedTypedNoticeResponse(
              requestKvp.Key,
              requestKvp.Value.Stream,
              requestKvp.Value.Notice,
              bf.Notice,
              bf.ContentType,
              bf.Metadata,
              bf.Exception
            );
        foreach (BatchRoutedTypedNoticeResponse failure in mappedFailures)
        {
          failures.Add(failure);
        }

        IEnumerable<BatchRoutedTypedNoticeResponse> mappedSuccesses = from bs in batchIoResult.Successes
          join requestKvp in notices
            on bs.BatchNoticeId equals requestKvp.Key
          select new BatchRoutedTypedNoticeResponse(
            requestKvp.Key,
            requestKvp.Value.Stream,
            requestKvp.Value.Notice,
            bs.Notice,
            contentType: null,
            metadata: null,
            exception: null
          );
        foreach (BatchRoutedTypedNoticeResponse batchRoutedTypedNotice in mappedSuccesses)
        {
          successes.Add(batchRoutedTypedNotice);
        }
      }
      else
      {
        try
        {
          // We don't have a batch io, so we have to invoke multiple single-notice dispatches.
          await Parallel.ForEachAsync(
            validatedAndRouted,
            new ParallelOptions
            {
              MaxDegreeOfParallelism = batchDispatchOptions?.MaxDegreeOfParallelism ?? 1,
              CancellationToken = cancellationToken
            },
            async (batchRoutedTypedNotice, token) =>
            {
              try
              {
                token.ThrowIfCancellationRequested();
                // NOTE: We're invoking the dispatcher provider here (within the parallel invocation) so that we
                //   have the opportunity to use distinct instances of the dispatcher for each parallel invocation.
                //   This can be useful if the dispatcher is not thread-safe.
                IoNoticeDispatchResult response = await ioDispatcherProvider()
                  .DispatchAsync(
                    new IoNoticeDispatchRequest(
                      batchRoutedTypedNotice.Stream,
                      batchRoutedTypedNotice.Notice,
                      batchRoutedTypedNotice.Metadata,
                      batchRoutedTypedNotice.ContentType
                    ),
                    token
                  );
                successes.Add(
                  batchRoutedTypedNotice
                );
              }
              catch (Exception exception)
              {
                failures.Add(
                  new BatchRoutedTypedNoticeResponse(
                    batchRoutedTypedNotice.BatchNoticeId,
                    batchRoutedTypedNotice.Stream,
                    batchRoutedTypedNotice.TypedNotice,
                    batchRoutedTypedNotice.Notice,
                    batchRoutedTypedNotice.ContentType,
                    batchRoutedTypedNotice.Metadata,
                    exception
                  )
                );
              }
            }
          );
        }
        catch (Exception exception)
        {
          IEnumerable<BatchRoutedTypedNoticeResponse> notHandled = validatedAndRouted.Where(kvp =>
            failures.All(failure => failure.BatchNoticeId != kvp.BatchNoticeId) &&
            successes.All(success => success.BatchNoticeId != kvp.BatchNoticeId)
          );
          foreach (BatchRoutedTypedNoticeResponse batchRoutedTypedNotice in notHandled)
          {
            failures.Add(
              new BatchRoutedTypedNoticeResponse(
                batchRoutedTypedNotice.BatchNoticeId,
                batchRoutedTypedNotice.Stream,
                batchRoutedTypedNotice.TypedNotice,
                batchRoutedTypedNotice.Notice,
                batchRoutedTypedNotice.ContentType,
                batchRoutedTypedNotice.Metadata,
                exception
              )
            );
          }
        }
      }
    }

    return new BatchTypedNoticeDispatchResult(failures.Concat(successes));
  }
}
