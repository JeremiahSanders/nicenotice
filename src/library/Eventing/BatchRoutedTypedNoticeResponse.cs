namespace Jds.NiceNotice;

/// <summary>
///   A routed typed notice which is part of a batch.
/// </summary>
/// <remarks>
///   This type is intended to be a response type after batch processing.
///   This type is not used in request parameters as a design decision,
///   preferring to reinforce uniqueness requirements of <paramref name="BatchNoticeId" /> within the batch
///   by use of dictionaries.
/// </remarks>
/// <param name="BatchNoticeId">The unique identifier for this notice within its batch.</param>
/// <param name="Stream">The logical stream to which the notice is dispatched.</param>
/// <param name="Notice">The typed notice (<see cref="object" />) which is dispatched.</param>
/// <param name="SerializedNotice">
///   The serialized representation of the notice, as returned by the <see cref="INoticeIo" />.
/// </param>
public record BatchRoutedTypedNoticeResponse(
  string BatchNoticeId,
  EventStreamId Stream,
  object Notice,
  string SerializedNotice
);
