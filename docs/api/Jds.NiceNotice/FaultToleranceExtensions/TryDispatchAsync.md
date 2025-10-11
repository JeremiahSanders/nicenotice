# FaultToleranceExtensions.TryDispatchAsync method (1 of 4)

Asynchronously sends a notification using this I/O dispatcher, catching exceptions.

```csharp
public static Task<string?> TryDispatchAsync(this INoticeIo noticeIo, EventStreamId stream, 
    string notice, Action<string, Exception>? exceptionHandler = null, 
    CancellationToken cancellationToken = default)
```

| parameter | description |
| --- | --- |
| noticeIo | This instance of notice I/O. |
| stream | A logical notice event stream identifier. |
| notice | A notice to be dispatched. |
| exceptionHandler | Optional. Executed if an exception occurs when dispatching the notice. |
| cancellationToken | Optional. An asynchronous operation cancellation token. |

## Return Value

Returns the result of the asynchronous operation, when successful, or null upon failure.

## Remarks

Invokes [`DispatchAsync`](../INoticeIo/DispatchAsync.md) and catches any exceptions thrown. If an exception is thrown, the *exceptionHandler* is invoked with the notice and the exception.

## See Also

* interface [INoticeIo](../INoticeIo.md)
* struct [EventStreamId](../EventStreamId.md)
* class [FaultToleranceExtensions](../FaultToleranceExtensions.md)
* namespace [Jds.NiceNotice](../../NiceNotice.md)

---

# FaultToleranceExtensions.TryDispatchAsync&lt;TNotice&gt; method (2 of 4)

Asynchronously sends a notification to the configured I/O dispatcher, catching exceptions.

```csharp
public static Task<TypedNoticeDispatchResult<TNotice>?> TryDispatchAsync<TNotice>(
    this ITypedNoticeDispatcher dispatcher, TNotice notice, 
    Action<TNotice, Exception>? exceptionHandler = null, bool dispatchToFullNameStream = false, 
    CancellationToken cancellationToken = default)
```

| parameter | description |
| --- | --- |
| TNotice | A notice object type. |
| dispatcher | This notice dispatcher, which will handle the dispatch operation. |
| notice | The notice to be dispatched. |
| exceptionHandler | Optional. Executed if an exception occurs when dispatching the notice. |
| dispatchToFullNameStream | A value indicating whether the stream name is based on the type name (e.g., `MyEvent`) or the full type name (e.g., `MyCompany.MyApp.MyEvent`). |
| cancellationToken | Optional. An asynchronous operation cancellation token. |

## Return Value

A task that represents the asynchronous operation. The task result contains the dispatch result.

## Remarks

Invokes [`DispatchAsync`](../TypedNoticeDispatcherRoutingExtensions/DispatchAsync.md) and catches any exceptions thrown. If an exception is thrown, the *exceptionHandler* is invoked with the notice and the exception.

## See Also

* record [TypedNoticeDispatchResult&lt;TEventType&gt;](../TypedNoticeDispatchResult-1.md)
* interface [ITypedNoticeDispatcher](../ITypedNoticeDispatcher.md)
* class [FaultToleranceExtensions](../FaultToleranceExtensions.md)
* namespace [Jds.NiceNotice](../../NiceNotice.md)

---

# FaultToleranceExtensions.TryDispatchAsync&lt;TNotice&gt; method (3 of 4)

Asynchronously sends a notification to the configured I/O dispatcher, catching exceptions.

```csharp
public static Task<TypedNoticeDispatchResult<TNotice>?> TryDispatchAsync<TNotice>(
    this ITypedNoticeDispatcher dispatcher, TNotice notice, EventStreamId eventStreamId, 
    Action<TNotice, Exception>? exceptionHandler = null, 
    CancellationToken cancellationToken = default)
```

| parameter | description |
| --- | --- |
| TNotice | A notice object type. |
| dispatcher | This notice dispatcher, which will handle the dispatch operation. |
| notice | The notice to be dispatched. |
| eventStreamId | The logical stream to which the notification will be sent. |
| exceptionHandler | Optional. Executed if an exception occurs when dispatching the notice. |
| cancellationToken | Optional. An asynchronous operation cancellation token. |

## Return Value

A task that represents the asynchronous operation. The task result contains the dispatch result.

## Remarks

Invokes [`DispatchAsync`](../TypedNoticeDispatcherRoutingExtensions/DispatchAsync.md) and catches any exceptions thrown. If an exception is thrown, the *exceptionHandler* is invoked with the notice and the exception.

## See Also

* record [TypedNoticeDispatchResult&lt;TEventType&gt;](../TypedNoticeDispatchResult-1.md)
* interface [ITypedNoticeDispatcher](../ITypedNoticeDispatcher.md)
* struct [EventStreamId](../EventStreamId.md)
* class [FaultToleranceExtensions](../FaultToleranceExtensions.md)
* namespace [Jds.NiceNotice](../../NiceNotice.md)

---

# FaultToleranceExtensions.TryDispatchAsync&lt;TBaseNotice,TNotice&gt; method (4 of 4)

Asynchronously sends a notification to the configured I/O dispatcher, catching exceptions.

```csharp
public static Task<TypedNoticeDispatchResult<TNotice>?> TryDispatchAsync<TBaseNotice, TNotice>(
    this ITypedNoticeDispatcher<TBaseNotice> dispatcher, TNotice notice, 
    Action<TNotice, Exception>? exceptionHandler = null, 
    CancellationToken cancellationToken = default)
    where TNotice : TBaseNotice
```

| parameter | description |
| --- | --- |
| TBaseNotice | A base notice type of *TNotice*. |
| TNotice | A notice type which is being dispatched. |
| dispatcher | This typed notice dispatcher. |
| notice | A notice to be dispatched. |
| exceptionHandler | Optional. Executed if an exception occurs when dispatching the notice. |
| cancellationToken | Optional. An asynchronous operation cancellation token. |

## Return Value

Returns the result of the asynchronous operation, when successful, or null upon failure.

## Remarks

Invokes [`DispatchAsync`](../ITypedNoticeDispatcher-1/DispatchAsync.md) and catches any exceptions thrown. If an exception is thrown, the *exceptionHandler* is invoked with the notice and the exception.

## See Also

* record [TypedNoticeDispatchResult&lt;TEventType&gt;](../TypedNoticeDispatchResult-1.md)
* interface [ITypedNoticeDispatcher&lt;TEnterpriseEventBaseType&gt;](../ITypedNoticeDispatcher-1.md)
* class [FaultToleranceExtensions](../FaultToleranceExtensions.md)
* namespace [Jds.NiceNotice](../../NiceNotice.md)

<!-- DO NOT EDIT: generated by xmldocmd for NiceNotice.dll -->
