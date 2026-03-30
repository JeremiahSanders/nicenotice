using Jds.NiceNotice.Dispatching;

namespace Jds.NiceNotice;

/// <summary>
///   A request to dispatch a batch of notices to I/O.
/// </summary>
/// <param name="notices">A collection of routed notices to dispatch.</param>
/// <param name="batchDispatchOptions">Optional. Options configuring batch dispatch.</param>
public class IoBatchNoticeDispatchRequest(
  IReadOnlyDictionary<string, IoNoticeDispatchRequest> notices,
  BatchDispatchOptions? batchDispatchOptions = null
)
{
  /// <summary>
  ///   Gets the options configuring batch dispatch.
  /// </summary>
  public BatchDispatchOptions? BatchDispatchOptions { get; init; } = batchDispatchOptions;

  /// <summary>
  ///   Gets the notices to dispatch, keyed with an identity for it within the batch.
  /// </summary>
  public IReadOnlyDictionary<string, IoNoticeDispatchRequest> Notices { get; init; } = notices;

  /// <inheritdoc />
  public override bool Equals(object? obj)
  {
    if (obj is null)
    {
      return false;
    }

    if (ReferenceEquals(this, obj))
    {
      return true;
    }

    if (obj.GetType() != GetType())
    {
      return false;
    }

    return Equals((IoBatchNoticeDispatchRequest)obj);
  }

  /// <inheritdoc />
  public override int GetHashCode()
  {
    return HashCode.Combine(BatchDispatchOptions, Notices);
  }

  /// <summary>
  ///   Determines whether the specified <see cref="IoBatchNoticeDispatchRequest" /> is equal to the current <see cref="IoBatchNoticeDispatchRequest" />.
  /// </summary>
  /// <param name="other">Another request to compare with this instance.</param>
  /// <returns>Returns <c>true</c> if the specified request is equal to this instance; otherwise, <c>false</c>.</returns>
  protected bool Equals(IoBatchNoticeDispatchRequest other)
  {
    return Equals(BatchDispatchOptions, other.BatchDispatchOptions)
           && (Notices.Equals(other.Notices) || (
             Notices.Keys.SequenceEqual(other.Notices.Keys) &&
             Notices.Values.SequenceEqual(other.Notices.Values)
           ));
  }
}
