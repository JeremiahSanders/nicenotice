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
    INoticeIo ioDispatcher,
    Func<string, BatchRoutedTypedNoticeRequest,
      Either<(BatchRoutedTypedNoticeResponse, Exception), BatchRoutedTypedNoticeResponse>> trySerializeAndValidate,
    IReadOnlyDictionary<string, BatchRoutedTypedNoticeRequest> notices,
    BatchDispatchOptions? batchDispatchOptions = null,
    CancellationToken cancellationToken = default)
  {
    ConcurrentBag<(BatchRoutedTypedNoticeResponse, Exception)> failures = [];
    ConcurrentBag<BatchRoutedTypedNoticeResponse> successes = [];


    (List<(BatchRoutedTypedNoticeResponse, Exception)> lefts, List<BatchRoutedTypedNoticeResponse> rights) routed =
      notices
        .Select(notice =>
          Eithers
            .Try(() =>
              trySerializeAndValidate(notice.Key, notice.Value)
            )
            .Match(
              exception => Eithers.Left<(BatchRoutedTypedNoticeResponse, Exception), BatchRoutedTypedNoticeResponse>(
                (new BatchRoutedTypedNoticeResponse(notice.Key, notice.Value.Stream, notice.Value.Notice, string.Empty),
                  exception)
              ),
              static success => success
            )
        )
        .ToList()
        .Partition();
    routed.lefts.ForEach(failure => failures.Add(failure));

    if (routed.rights.Count > 0)
    {
      if (ioDispatcher is INoticeBatchIo batchIo)
      {
        BatchIoNoticeDispatchResult batchIoResult = (await Eithers.TryAsync(async () =>
            await batchIo.DispatchNoticesAsync(
              routed
                .rights
                .ToDictionary(
                  static rtn => rtn.BatchNoticeId,
                  static rtn => new BatchedIoRequestNotice(rtn.Stream, rtn.SerializedNotice)
                ),
              batchDispatchOptions,
              cancellationToken
            )
          ))
          .MapLeft(exception => new BatchIoNoticeDispatchResult
            {
              Failures = routed
                .rights.Select(n =>
                  (new BatchedIoResponseNotice(n.BatchNoticeId, n.Stream, n.SerializedNotice), exception)
                )
                .ToList(),
              Successes = []
            }
          )
          .Match(i => i, i => i);

        IEnumerable<(BatchRoutedTypedNoticeResponse, Exception)> mappedFailures =
          from bf in batchIoResult.Failures
          join requestKvp in notices
            on bf.Item1.BatchNoticeId equals requestKvp.Key
          select
            (new BatchRoutedTypedNoticeResponse(requestKvp.Key, requestKvp.Value.Stream, requestKvp.Value.Notice, bf.Item1.Notice),
              bf.Item2);
        foreach ((BatchRoutedTypedNoticeResponse, Exception) failure in mappedFailures)
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
            bs.Notice
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
            routed.rights,
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
                string response = await ioDispatcher.DispatchAsync(
                  batchRoutedTypedNotice.Stream,
                  batchRoutedTypedNotice.SerializedNotice,
                  token
                );
                successes.Add(
                  batchRoutedTypedNotice with
                  {
                    SerializedNotice = response
                  }
                );
              }
              catch (Exception exception)
              {
                failures.Add((batchRoutedTypedNotice, exception));
              }
            }
          );
        }
        catch (Exception exception)
        {
          IEnumerable<BatchRoutedTypedNoticeResponse> notHandled = routed.rights.Where(kvp =>
            failures.All(failure => failure.Item1.BatchNoticeId != kvp.BatchNoticeId) &&
            successes.All(success => success.BatchNoticeId != kvp.BatchNoticeId)
          );
          foreach (BatchRoutedTypedNoticeResponse batchRoutedTypedNotice in notHandled)
          {
            failures.Add(new ValueTuple<BatchRoutedTypedNoticeResponse, Exception>(batchRoutedTypedNotice, exception));
          }
        }
      }
    }

    return new BatchTypedNoticeDispatchResult
    {
      Failures = failures.ToList(),
      Successes = successes.ToList()
    };
  }
}
