# NiceNotice assembly

## Jds.NiceNotice namespace

| public type | description |
| --- | --- |
| class [BatchIoRequest](./Jds.NiceNotice/BatchIoRequest.md) | A request to dispatch a batch of notices to I/O. |
| record [EnterpriseEvent](./Jds.NiceNotice/EnterpriseEvent.md) | A base enterprise event (data transfer object), suitable for extending with application-specific properties. |
| struct [EventStreamId](./Jds.NiceNotice/EventStreamId.md) | Represents a unique identifier for an event stream. This type is used to uniquely identify and manage event streams, ensuring type safety when working with specific streams in the application's event notification system. |
| interface [INoticeBatchIo](./Jds.NiceNotice/INoticeBatchIo.md) | A [`INoticeIo`](./Jds.NiceNotice/INoticeIo.md) that can dispatch batches of notices. |
| interface [INoticeIo](./Jds.NiceNotice/INoticeIo.md) | Represents a dispatcher responsible for sending enterprise event notices to specific event streams. |
| interface [INoticeMetadata](./Jds.NiceNotice/INoticeMetadata.md) | Interface which can be applied to typed notices to generate custom metadata which can be included when being dispatched to I/O. |
| class [IoNoticeDispatchResult](./Jds.NiceNotice/IoNoticeDispatchResult.md) | A result of dispatching a notice to I/O. |
| class [IoRequestNotice](./Jds.NiceNotice/IoRequestNotice.md) | A notice I/O request, which may be part of a batch. |
| interface [ITypedNoticeDispatcher&lt;TEnterpriseEventBaseType&gt;](./Jds.NiceNotice/ITypedNoticeDispatcher-1.md) | Defines an interface for dispatching enterprise events of specified base types to logical event streams. The default implementation applies (in order): (1) logical routing, (2) serialization, (3) validation, and (4) dispatch to I/O using an [`INoticeIo`](./Jds.NiceNotice/INoticeIo.md). |
| interface [ITypedNoticeDispatcher](./Jds.NiceNotice/ITypedNoticeDispatcher.md) | Defines an interface for dispatching enterprise events to logical event streams. The default implementation applies (in order): (1) serialization, (2) validation, and (3) dispatch to I/O using an [`INoticeIo`](./Jds.NiceNotice/INoticeIo.md). |
| static class [JsonDefaults](./Jds.NiceNotice/JsonDefaults.md) | Provides default JSON serialization settings for the library. |
| static class [NoticeIoJsonExtensions](./Jds.NiceNotice/NoticeIoJsonExtensions.md) | Provides extension methods for the [`INoticeIo`](./Jds.NiceNotice/INoticeIo.md) interface to dispatch notifications serialized as JSON. |
| static class [ServiceCollectionExtensions](./Jds.NiceNotice/ServiceCollectionExtensions.md) | Methods extending IServiceCollection to add cross-app notifications services. |
| static class [TypedNoticeDispatcherBatchExtensions](./Jds.NiceNotice/TypedNoticeDispatcherBatchExtensions.md) | Extensions to typed notice dispatchers supporting batch notice dispatch. |
| static class [TypedNoticeDispatcherFaultToleranceExtensions](./Jds.NiceNotice/TypedNoticeDispatcherFaultToleranceExtensions.md) | Methods extending [`ITypedNoticeDispatcher`](./Jds.NiceNotice/ITypedNoticeDispatcher.md) and [`ITypedNoticeDispatcher`](./Jds.NiceNotice/ITypedNoticeDispatcher-1.md) supporting fault tolerance. |
| static class [TypedNoticeDispatcherRoutingExtensions](./Jds.NiceNotice/TypedNoticeDispatcherRoutingExtensions.md) | Methods extending [`ITypedNoticeDispatcher`](./Jds.NiceNotice/ITypedNoticeDispatcher.md) to support additional typed notice dispatch patterns. |

## Jds.NiceNotice.Configuration namespace

| public type | description |
| --- | --- |
| class [MissingDependencyException](./Jds.NiceNotice.Configuration/MissingDependencyException.md) | Represents an exception thrown when a required dependency is missing. |
| class [NiceNoticeBuilder](./Jds.NiceNotice.Configuration/NiceNoticeBuilder.md) | A fluent builder for configuring NiceNotice services. |
| class [TypedNoticesBuilder&lt;TEnterpriseEventBaseType&gt;](./Jds.NiceNotice.Configuration/TypedNoticesBuilder-1.md) | A builder for configuring typed notices (enterprise events) which derive from a base type. |
| class [TypedNoticesBuilderOptions](./Jds.NiceNotice.Configuration/TypedNoticesBuilderOptions.md) | Configuration-based setup parameters for typed notices. |

## Jds.NiceNotice.Dispatching namespace

| public type | description |
| --- | --- |
| record [BatchDispatchOptions](./Jds.NiceNotice.Dispatching/BatchDispatchOptions.md) | Options configuring the behavior of batch dispatch. |
| class [BatchedIoResponseNotice](./Jds.NiceNotice.Dispatching/BatchedIoResponseNotice.md) | A routed notice that is part of a batch I/O response. |
| class [BatchIoNoticeDispatchResult](./Jds.NiceNotice.Dispatching/BatchIoNoticeDispatchResult.md) | The result of dispatching a batch of notices. |

## Jds.NiceNotice.Dispatching.Implementations namespace

| public type | description |
| --- | --- |
| class [CapturingNoticeIo](./Jds.NiceNotice.Dispatching.Implementations/CapturingNoticeIo.md) | A thread-safe implementation of [`INoticeIo`](./Jds.NiceNotice/INoticeIo.md) which is intended for test purposes. Each dispatched notice is captured and can be retrieved via [`CapturedNotices`](./Jds.NiceNotice.Dispatching.Implementations/CapturingNoticeIo/CapturedNotices.md). |
| class [NullNoticeIo](./Jds.NiceNotice.Dispatching.Implementations/NullNoticeIo.md) | Provides a no-operation implementation of [`INoticeIo`](./Jds.NiceNotice/INoticeIo.md), primarily used as a default or placeholder where event dispatching is not required. |

## Jds.NiceNotice.TypedNotices namespace

| public type | description |
| --- | --- |
| class [BatchRoutedTypedNoticeRequest](./Jds.NiceNotice.TypedNotices/BatchRoutedTypedNoticeRequest.md) | A routed notice that is part of a batch. |
| class [BatchRoutedTypedNoticeResponse](./Jds.NiceNotice.TypedNotices/BatchRoutedTypedNoticeResponse.md) | A routed typed notice which is part of a batch. |
| class [BatchTypedNoticeDispatchResult](./Jds.NiceNotice.TypedNotices/BatchTypedNoticeDispatchResult.md) | The result of dispatching a batch of typed notices. |
| class [DispatchBatchRequest&lt;TBaseEnterpriseEvent&gt;](./Jds.NiceNotice.TypedNotices/DispatchBatchRequest-1.md) | A request to dispatch a batch of notices. |
| class [DispatchBatchRequest](./Jds.NiceNotice.TypedNotices/DispatchBatchRequest.md) | A request to dispatch a batch of notices. |
| class [NoticeStreamAttribute](./Jds.NiceNotice.TypedNotices/NoticeStreamAttribute.md) | An attribute indicating the logical identifier of the notice stream to which a notice type should be sent. I.e., the [`EventStreamId`](./Jds.NiceNotice/EventStreamId.md) value. |
| abstract class [TypedNoticeDispatcher&lt;TEnterpriseEventBaseType&gt;](./Jds.NiceNotice.TypedNotices/TypedNoticeDispatcher-1.md) | Represents an abstract base class for dispatching notifications of a specified type. |
| abstract class [TypedNoticeDispatcher](./Jds.NiceNotice.TypedNotices/TypedNoticeDispatcher.md) | A base class implementation of [`ITypedNoticeDispatcher`](./Jds.NiceNotice/ITypedNoticeDispatcher.md). Provides `abstract` and `virtual` methods for customizing its behavior. |
| record [TypedNoticeDispatchResult&lt;TEventType&gt;](./Jds.NiceNotice.TypedNotices/TypedNoticeDispatchResult-1.md) | The result of dispatching a typed notice. |

## Jds.NiceNotice.TypedNotices.Metadata namespace

| public type | description |
| --- | --- |
| static class [MetadataProviders](./Jds.NiceNotice.TypedNotices.Metadata/MetadataProviders.md) | Implementation of [`NoticeMetadataProvider`](./Jds.NiceNotice.TypedNotices.Metadata/NoticeMetadataProvider.md). |
| abstract class [NoticeMetadataProvider&lt;TEnterpriseEventBaseType&gt;](./Jds.NiceNotice.TypedNotices.Metadata/NoticeMetadataProvider-1.md) | An abstraction representing the algorithm used for extracting metadata from a notice. |
| abstract class [NoticeMetadataProvider](./Jds.NiceNotice.TypedNotices.Metadata/NoticeMetadataProvider.md) | An abstraction representing the algorithm used for extracting metadata from a notice. |

## Jds.NiceNotice.TypedNotices.Routing namespace

| public type | description |
| --- | --- |
| abstract class [NoticeRouter&lt;TEnterpriseEventBaseType&gt;](./Jds.NiceNotice.TypedNotices.Routing/NoticeRouter-1.md) | An abstraction representing the algorithm used for selecting a logical event stream (identified by [`EventStreamId`](./Jds.NiceNotice/EventStreamId.md)) for a notice, a process referred to as routing. |
| class [NoticeRoutingException](./Jds.NiceNotice.TypedNotices.Routing/NoticeRoutingException.md) | Represents an exception which is thrown when unable to determine the event stream to which an enterprise event should be dispatched. |
| static class [Routers](./Jds.NiceNotice.TypedNotices.Routing/Routers.md) | Constructors for enterprise event stream selectors. |

## Jds.NiceNotice.TypedNotices.Serialization namespace

| public type | description |
| --- | --- |
| class [NoticeSerializationException](./Jds.NiceNotice.TypedNotices.Serialization/NoticeSerializationException.md) | Represents an exception which is thrown when a failure occurs during the serialization of a typed notice. |
| abstract class [NoticeSerializer&lt;TEnterpriseEventBaseType&gt;](./Jds.NiceNotice.TypedNotices.Serialization/NoticeSerializer-1.md) | An abstraction representing the algorithm used for serializing a notice to a string. |
| abstract class [NoticeSerializer](./Jds.NiceNotice.TypedNotices.Serialization/NoticeSerializer.md) | An abstraction representing the algorithm used for serializing a notice to a string. |
| static class [Serializers](./Jds.NiceNotice.TypedNotices.Serialization/Serializers.md) | Methods arranging implementations of [`NoticeSerializer`](./Jds.NiceNotice.TypedNotices.Serialization/NoticeSerializer.md). |

## Jds.NiceNotice.TypedNotices.Validation namespace

| public type | description |
| --- | --- |
| static class [DataAnnotationsValidation](./Jds.NiceNotice.TypedNotices.Validation/DataAnnotationsValidation.md) | Helper methods for validating notifications using Validator. |
| class [NoticeValidationException](./Jds.NiceNotice.TypedNotices.Validation/NoticeValidationException.md) | Represents an exception thrown when a validation operation fails during the dispatch of an enterprise event. |
| abstract class [NoticeValidator&lt;TEnterpriseEventBaseType&gt;](./Jds.NiceNotice.TypedNotices.Validation/NoticeValidator-1.md) | An abstraction representing the algorithm used for validating a notice. |
| abstract class [NoticeValidator](./Jds.NiceNotice.TypedNotices.Validation/NoticeValidator.md) | An abstraction representing the algorithm used for validating a notice. |
| static class [Validators](./Jds.NiceNotice.TypedNotices.Validation/Validators.md) | Methods for creating notification validators. |

<!-- DO NOT EDIT: generated by xmldocmd for NiceNotice.dll -->
