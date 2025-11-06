# NiceNotice assembly

## Jds.NiceNotice namespace

| public type | description |
| --- | --- |
| record [BatchDispatchOptions](./Jds.NiceNotice/BatchDispatchOptions.md) | Options configuring the behavior of batch dispatch. |
| class [BatchedIoRequestNotice](./Jds.NiceNotice/BatchedIoRequestNotice.md) | A notice that is part of a batch I/O request. |
| record [BatchedIoResponseNotice](./Jds.NiceNotice/BatchedIoResponseNotice.md) | A routed notice that is part of a batch I/O response. |
| class [BatchedRoutedTypedNotice](./Jds.NiceNotice/BatchedRoutedTypedNotice.md) | A routed notice that is part of a batch. |
| class [BatchIoNoticeDispatchResult](./Jds.NiceNotice/BatchIoNoticeDispatchResult.md) | The result of dispatching a batch of notices. |
| record [BatchRoutedTypedNoticeResponse](./Jds.NiceNotice/BatchRoutedTypedNoticeResponse.md) | A routed typed notice which is part of a batch. |
| class [BatchTypedNoticeDispatchResult](./Jds.NiceNotice/BatchTypedNoticeDispatchResult.md) | The result of dispatching a batch of typed notices. |
| class [CapturingNoticeIo](./Jds.NiceNotice/CapturingNoticeIo.md) | A thread-safe implementation of [`INoticeIo`](./Jds.NiceNotice/INoticeIo.md) which is intended for test purposes. Each dispatched notice is captured and can be retrieved via [`CapturedNotices`](./Jds.NiceNotice/CapturingNoticeIo/CapturedNotices.md). |
| class [DataAnnotationsValidator&lt;TEnterpriseEventBaseType&gt;](./Jds.NiceNotice/DataAnnotationsValidator-1.md) | An implementation of [`NoticeValidator`](./Jds.NiceNotice/NoticeValidator-1.md) that uses Validator to validate the enterprise event notice. |
| class [DataAnnotationsValidator](./Jds.NiceNotice/DataAnnotationsValidator.md) | An implementation of [`NoticeValidator`](./Jds.NiceNotice/NoticeValidator.md) that uses Validator to validate the enterprise event notice. |
| class [DelegateNoticeValidator&lt;TEnterpriseEventBaseType&gt;](./Jds.NiceNotice/DelegateNoticeValidator-1.md) | An implementation of [`NoticeValidator`](./Jds.NiceNotice/NoticeValidator-1.md) that uses a delegate to validate the enterprise event notice. |
| class [DelegateNoticeValidator](./Jds.NiceNotice/DelegateNoticeValidator.md) | An implementation of [`NoticeValidator`](./Jds.NiceNotice/NoticeValidator.md) that uses a delegate to validate the enterprise event notice. |
| class [DispatchBatchRequest&lt;TBaseEnterpriseEvent&gt;](./Jds.NiceNotice/DispatchBatchRequest-1.md) | A request to dispatch a batch of notices. |
| class [DispatchBatchRequest](./Jds.NiceNotice/DispatchBatchRequest.md) | A request to dispatch a batch of notices. |
| record [EnterpriseEvent](./Jds.NiceNotice/EnterpriseEvent.md) | A base enterprise event (data transfer object), suitable for extending with application-specific properties. |
| struct [EventStreamId](./Jds.NiceNotice/EventStreamId.md) | Represents a unique identifier for an event stream. This type is used to uniquely identify and manage event streams, ensuring type safety when working with specific streams in the application's event notification system. |
| static class [FaultToleranceExtensions](./Jds.NiceNotice/FaultToleranceExtensions.md) | Extension methods supporting fault tolerance. |
| interface [INoticeBatchIo](./Jds.NiceNotice/INoticeBatchIo.md) | A [`INoticeIo`](./Jds.NiceNotice/INoticeIo.md) that can dispatch batches of notices. |
| interface [INoticeIo](./Jds.NiceNotice/INoticeIo.md) | Represents a dispatcher responsible for sending enterprise event notices to specific event streams. |
| interface [ITypedNoticeDispatcher&lt;TEnterpriseEventBaseType&gt;](./Jds.NiceNotice/ITypedNoticeDispatcher-1.md) | Defines an interface for dispatching enterprise events of specified base types to logical event streams. The default implementation applies (in order): (1) logical routing, (2) serialization, (3) validation, and (4) dispatch to I/O using an [`INoticeIo`](./Jds.NiceNotice/INoticeIo.md). |
| interface [ITypedNoticeDispatcher](./Jds.NiceNotice/ITypedNoticeDispatcher.md) | Defines an interface for dispatching enterprise events to logical event streams. The default implementation applies (in order): (1) serialization, (2) validation, and (3) dispatch to I/O using an [`INoticeIo`](./Jds.NiceNotice/INoticeIo.md). |
| static class [JsonDefaults](./Jds.NiceNotice/JsonDefaults.md) | Provides default JSON serialization settings for the library. |
| class [MissingDependencyException](./Jds.NiceNotice/MissingDependencyException.md) | Represents an exception thrown when a required dependency is missing. |
| class [NiceNoticeBuilder](./Jds.NiceNotice/NiceNoticeBuilder.md) | A fluent builder for configuring NiceNotice services. |
| class [NoOpNoticeValidator&lt;TEnterpriseEventBaseType&gt;](./Jds.NiceNotice/NoOpNoticeValidator-1.md) | A notice validator that does not perform any validation (all notices are considered valid). |
| class [NoOpNoticeValidator](./Jds.NiceNotice/NoOpNoticeValidator.md) | A notice validator that does not perform any validation (all notices are considered valid). |
| static class [NoticeIoJsonExtensions](./Jds.NiceNotice/NoticeIoJsonExtensions.md) | Provides extension methods for the [`INoticeIo`](./Jds.NiceNotice/INoticeIo.md) interface to dispatch notifications serialized as JSON. |
| abstract class [NoticeSerializer&lt;TEnterpriseEventBaseType&gt;](./Jds.NiceNotice/NoticeSerializer-1.md) | An abstraction representing the algorithm used for serializing a notice to a string. |
| abstract class [NoticeSerializer](./Jds.NiceNotice/NoticeSerializer.md) | An abstraction representing the algorithm used for serializing a notice to a string. |
| abstract class [NoticeStreamSelector&lt;TEnterpriseEventBaseType&gt;](./Jds.NiceNotice/NoticeStreamSelector-1.md) | An abstraction representing the algorithm used for selecting a logical event stream (identified by [`EventStreamId`](./Jds.NiceNotice/EventStreamId.md)) for a notice, a process sometimes referred to as routing. |
| class [NoticeValidationException](./Jds.NiceNotice/NoticeValidationException.md) | Represents an exception thrown when a validation operation fails during the dispatch of an enterprise event. |
| abstract class [NoticeValidator&lt;TEnterpriseEventBaseType&gt;](./Jds.NiceNotice/NoticeValidator-1.md) | An abstraction representing the algorithm used for validating a notice. |
| abstract class [NoticeValidator](./Jds.NiceNotice/NoticeValidator.md) | An abstraction representing the algorithm used for validating a notice. |
| class [NullNoticeIo](./Jds.NiceNotice/NullNoticeIo.md) | Provides a no-operation implementation of [`INoticeIo`](./Jds.NiceNotice/INoticeIo.md), primarily used as a default or placeholder where event dispatching is not required. |
| class [SerializationException](./Jds.NiceNotice/SerializationException.md) | Represents an exception which is thrown when a failure occurs during the serialization of an enterprise event. |
| static class [Serializers](./Jds.NiceNotice/Serializers.md) | Methods arranging implementations of [`NoticeSerializer`](./Jds.NiceNotice/NoticeSerializer.md). |
| static class [ServiceCollectionExtensions](./Jds.NiceNotice/ServiceCollectionExtensions.md) | Methods extending IServiceCollection to add cross-app notifications services. |
| static class [ServiceProviderExtensions](./Jds.NiceNotice/ServiceProviderExtensions.md) | Methods extending IServiceProvider to support cross-app notifications services. |
| class [StreamDeterminationException](./Jds.NiceNotice/StreamDeterminationException.md) | Represents an exception which is thrown when unable to determine the event stream to which an enterprise event should be dispatched. |
| static class [StreamSelectors](./Jds.NiceNotice/StreamSelectors.md) | Constructors for enterprise event stream selectors. |
| static class [TypedNoticeConfigurationExtensions](./Jds.NiceNotice/TypedNoticeConfigurationExtensions.md) | Methods extending the NiceNoticeBuilder to support configuration-based setup for typed notices. |
| abstract class [TypedNoticeDispatcher&lt;TEnterpriseEventBaseType&gt;](./Jds.NiceNotice/TypedNoticeDispatcher-1.md) | Represents an abstract base class for dispatching notifications of a specified type. |
| abstract class [TypedNoticeDispatcher](./Jds.NiceNotice/TypedNoticeDispatcher.md) | A base class implementation of [`ITypedNoticeDispatcher`](./Jds.NiceNotice/ITypedNoticeDispatcher.md). Provides `abstract` and `virtual` methods for customizing its behavior. |
| static class [TypedNoticeDispatcherBatchExtensions](./Jds.NiceNotice/TypedNoticeDispatcherBatchExtensions.md) | Extensions to typed notice dispatchers supporting batch notice dispatch. |
| static class [TypedNoticeDispatcherRoutingExtensions](./Jds.NiceNotice/TypedNoticeDispatcherRoutingExtensions.md) | Methods extending [`ITypedNoticeDispatcher`](./Jds.NiceNotice/ITypedNoticeDispatcher.md) to support additional typed notice dispatch patterns. |
| record [TypedNoticeDispatchResult&lt;TEventType&gt;](./Jds.NiceNotice/TypedNoticeDispatchResult-1.md) | The result of dispatching a typed notice. |
| class [TypedNoticesBuilder&lt;TEnterpriseEventBaseType&gt;](./Jds.NiceNotice/TypedNoticesBuilder-1.md) | A builder for configuring typed notices (enterprise events) which derive from a base type. |
| static class [Validators](./Jds.NiceNotice/Validators.md) | Methods for creating notification validators. |

<!-- DO NOT EDIT: generated by xmldocmd for NiceNotice.dll -->
