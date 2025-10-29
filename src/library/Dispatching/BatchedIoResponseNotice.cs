namespace Jds.NiceNotice;

/// <summary>
///   A routed notice that is part of a batch I/O response.
/// </summary>
/// <param name="BatchNoticeId">An identifier for this notice within the batch.</param>
/// <param name="Stream">A logical notification stream identifier.</param>
/// <param name="Notice">The content of the notification message.</param>
public record BatchedIoResponseNotice(string BatchNoticeId, EventStreamId Stream, string Notice);
