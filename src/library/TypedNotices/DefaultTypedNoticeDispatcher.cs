using Jds.NiceNotice.Dispatching;
using Jds.NiceNotice.TypedNotices.Routing;
using Jds.NiceNotice.TypedNotices.Serialization;
using Jds.NiceNotice.TypedNotices.Validation;

namespace Jds.NiceNotice.TypedNotices;

/// <summary>
///   Default implementation of <see cref="TypedNoticeDispatcher" />.
/// </summary>
/// <param name="ioDispatcherProvider">A function that returns an I/O dispatcher (which will send the serialized notices).</param>
/// <param name="noticeSerializer">A serializer for notices.</param>
/// <param name="noticeValidator">Optional. A validator for notices.</param>
internal class DefaultTypedNoticeDispatcher(
  Func<INoticeIo> ioDispatcherProvider,
  NoticeSerializer noticeSerializer,
  NoticeValidator? noticeValidator
) : TypedNoticeDispatcher(ioDispatcherProvider)
{
  /// <inheritdoc />
  protected override string SerializeNotice<TEventType>(TEventType notice)
  {
    return noticeSerializer.Serialize(notice);
  }

  /// <inheritdoc />
  protected override IReadOnlyList<string>? ValidateNotice<TEventType>(TEventType notice, string serializedNotice)
  {
    return noticeValidator?.Validate(notice, serializedNotice);
  }
}

/// <summary>
///   Default implementation of <see cref="TypedNoticeDispatcher{TEnterpriseEventBaseType}" />.
/// </summary>
/// <param name="ioDispatcherProvider">A function that returns an I/O dispatcher (which will send the serialized notices).</param>
/// <param name="router">A stream selector for routing notices.</param>
/// <param name="serializer">A serializer for notices.</param>
/// <param name="validator">Optional. A validator for notices.</param>
internal class DefaultTypedNoticeDispatcher<TEnterpriseEventBaseType>(
  Func<INoticeIo> ioDispatcherProvider,
  NoticeRouter<TEnterpriseEventBaseType> router,
  NoticeSerializer<TEnterpriseEventBaseType> serializer,
  NoticeValidator<TEnterpriseEventBaseType> validator
)
  : TypedNoticeDispatcher<TEnterpriseEventBaseType>(ioDispatcherProvider)
  where TEnterpriseEventBaseType : notnull
{
  /// <inheritdoc />
  protected override EventStreamId GetStreamId(TEnterpriseEventBaseType notice)
  {
    return router.GetStreamId(notice);
  }

  /// <inheritdoc />
  protected override string SerializeNotice<TEventType>(TEventType notice)
  {
    return serializer.Serialize(notice);
  }

  /// <inheritdoc />
  protected override IReadOnlyList<string>? ValidateNotice<TEventType>(TEventType notice, string serializedNotice)
  {
    return validator.Validate(notice, serializedNotice);
  }
}
