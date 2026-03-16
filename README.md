# NiceNotice

> NiceNotice is an abstraction layer for cross-application notifications.

NiceNotice is intended to help address the arrangement and coordination of the logical work required to dispatch **typed**/structured notifications of application events to an event bus, e.g., AWS Simple Notification Service (SNS), RabbitMQ.

Dispatching a typed notice is as simple as: `await dispatcher.DispatchAsync(new UserSessionStarted { SessionId = signInResult.SessionId, UserId = signInResult.UserId })`. (See [How the `ITypedNoticeDispatcher<TEnterpriseEventBaseType>` works][how-typednoticedispatcher-works] for details on how the dispatcher works.)

NiceNotice applies a structured approach to building an application/enterprise event notification dispatch system in .NET applications using four basic components:

* Routing
  * Routing answers the question "given an arbitrary event object, to what _logical destination_ (i.e., stream) does it belong."
* Serialization
  * Serialization encodes the event notification object into a string in a manner consistent with the documented expectations of your application and the event bus. (Generally, this just means serialize the object to JSON.)
* Validation
  * Validation answers the question "is this event object _and its serialized representation_ valid to send."
* Dispatch
  * Dispatch is the work to send the serialized notice to an external I/O (e.g., AWS SNS).

## Requirements

### Runtime Application Requirements

**.NET Generic Host**: NiceNotice is designed to be used in applications using the standard [.NET Generic Host infrastructure][dotnet-generic-host] (**standard in ASP.NET Core applications**; available to other applications using the [Microsoft.Extensions.Hosting][] NuGet package).

Technically, NiceNotice only depends upon the `IServiceCollection` and `IServiceProvider` infrastructure, but most applications using cross-application event notifications will be using service resolution from the application `Host`.

**External I/O Client**: You'll need to **install and configure** your chosen messaging I/O client, e.g., [AWS SDK for Simple Notification Service][awssdk-sns]. Make sure that the applicable I/O dependencies for that library are registered in the `Host` `ServiceCollection`. E.g., if you're using AWS SNS then you must make sure `IAmazonSimpleNotificationService` is registered. (See [Notice Dispatch][notice-dispatch] for an explanation of how NiceNotice represents/adapts an I/O client.)

## Getting Started

Start by installing [the `NiceNotice` NuGet package][nicenotice-nuget].

### Step 1: Create/Select your base/required event schema

> **This is a crucial decision point if your use case or organization depends upon a shared event schema.** (E.g., if _all_ notices need a `timestamp` or a deduplication `id`.)

In your application, declare a _base_ "enterprise event" type. This is a **value object** which **all** typed notices are expected to extend.

[`ExampleCustomBaseEnterpriseEvent` is defined in a NiceNotice unit test][enterprise-event-example-custom] and provides an example of how you might define a "base" event for your project.

#### Event Schema Object Quick Start

NiceNotice provides a recommended base `record`: `EnterpriseEvent`. The `EnterpriseEvent` type exposes three important properties: `schema`, `timestamp`, and `id`.

> The `schema` allows anyone reading the messages to know what properties the message should have. (There are `protected` properties which can be overriden to customize the value.)
>
> The `timestamp` allows messages to be ordered chronologically.
>
> The `id` allows messages to be deduplicated; an essential component in resilient asynchronous workflows.

Simply add a `record` in your project extending `EnterpriseEvent`.

> See [this `GameEvent` example from NiceNotice unit tests][enterprise-event-example-standard], intended to support an application sending events related to a team sport.

```csharp
public record MyApplicationEvent : EnterpriseEvent;
```

The above example _base_ application event, `MyApplicationEvent`, inherits `schema`, `timestamp`, and `id` properties from `EnterpriseEvent`. When serialized using the default NiceNotice serializer, an instance of `MyApplicationEvent` would be emitted like the following:

```json
{"$schema":"MyApplicationEvent","timestamp":"2025-10-11T23:17:59.5603648+00:00","id":"9fd368e0-23e2-4ec2-a30a-6ef99c3c2841"}
```

#### Routing Considerations

By default, NiceNotice internally routes notices to an `EventStreamId` which matches the notification **type name**. (E.g., `MyCompany.MyApplication.LoginEvent` would be routed to a "stream" named `LoginEvent`)

When you create a new event **type**, you can optionally add [a `[NoticeStream("")]` attribute][notice-stream-attribute] to specify a custom stream name.

**This is very helpful for "grouping" events.** For example, you might want all user session events (e.g., login, logout, timeout) to route to the same logical stream, `user-session`. To do so you'd simply add `[NoticeStream("user-session")]` to the applicable events' type definitions. This works on up the object hierarchy tree, so by applying a `[NoticeStream()]` attribute to a _base_ event type you can easily apply consistent routing to all event types derived from it.

### Step 2: Configure NiceNotice in application service configuration

On your `Host`'s `IServiceCollection`, use the `.AddNiceNotice()` extension method to add NiceNotice services.

> See [this example of how a console application might be configured][service-configuration-console-example].
>
> See [this example of how an ASP.NET Core application might be configured][service-configuration-webapi-example].

**Example using the builder fluent API:**

```csharp
services
  .AddNiceNotice(builder => builder
    .UseTypedNotices<MyApplicationEvent>(
      tnBuilder => tnBuilder
        .SerializeToJson() // Optional; JSON is the default
        .RouteToTypeNameStreams() // Optional; by default, type names are used as the stream names
        .ValidateWithDataAnnotations(),
      ServiceLifetime.Scoped
    )
    .DispatchToSns( // Dispatch enterprise events to AWS SNS. Requires `NiceNotice.Aws.Sns` NuGet package.
      "sns:topics", // IConfiguration section key, unique to this application's configuration; contains NiceNotice SNS topic routing
      ServiceLifetime.Scoped
    )
  );
```

**Example of using the configuration object API:**

```csharp
services
  .AddNiceNotice(builder => builder
    .UseTypedNotices<MyApplicationEvent>(
      new TypedNoticesBuilderOptions
      {
        RoutingType = TypedNoticesBuilderOptions.RoutingTypes.TypeFullName,
        SerializationType = TypedNoticesBuilderOptions.SerializationTypes.Json,
        ValidationType = TypedNoticesBuilderOptions.ValidationTypes.DataAttributes
      },
      ServiceLifetime.Scoped
    )
    .DispatchToSns( // Dispatch enterprise events to AWS SNS. Requires `NiceNotice.Aws.Sns` NuGet package.
      "sns:topics", // IConfiguration section key, unique to this application's configuration; contains NiceNotice SNS topic routing
      ServiceLifetime.Scoped
    )
  );
```

> The examples above use the NiceNotice Amazon Web Services (AWS) Simple Notification Service (SNS) dispatch library, [`NiceNotice.Aws.Sns`][nicenotice-aws-sdk-docs].
> In all examples, SNS notification dispatch was arranged to load runtime configuration from the configuration at key `sns:topics`.
> That _specific_ value is not required. Use the actual path **within your application's configuration** which contains SNS topic routing details.
>
> Refer to the [`NiceNotice.Aws.Sns`][nicenotice-aws-sdk-docs] documentation for more information.

### Step 3: Start sending notices

There are 2 interfaces exposed by NiceNotice which you can use to dispatch typed events: `ITypedNoticeDispatcher<TEnterpriseEventBaseType>` (generic) `ITypedNoticeDispatcher` (non-generic).

#### _Recommended_ `ITypedNoticeDispatcher<TEnterpriseEventBaseType>`

The `ITypedNoticeDispatcher<TEnterpriseEventBaseType>` interface is a central component in the overall strategy of sending typed notices (notifications with a known, serialized schema). Its main addition, over `ITypedNoticeDispatcher` (non-generic), is that it _constrains_ types, to help ensure message consistency.

**Example using the `MyApplicationEvent` base event, above:**

```csharp
# We're requesting a dispatcher using the base type we created
# and configured with `.UseTypedNotices<MyApplicationEvent>()`
public class MyService(ITypedNoticeDispatcher<MyApplicationEvent> dispatcher)
{
  public async Task SendImmediateNotice()
  {
    _ = await dispatcher.DispatchAsync(new ImportantNotice());
  }
  public record ImportantNotice : MyApplicationEvent;
}
```

#### `ITypedNoticeDispatcher`

The `ITypedNoticeDispatcher` encapsulates the idea of dispatching typed, serialized, validated notices. Its interface is very flexible, accepting notices of any `object` (_**required** to be a serializable data transfer object_).

_Note: `ITypedNoticeDispatcher<TEnterpriseEventBaseType>` is recommended if your organization or project implements a standard structured notification schema. It encourages consistent event notifications._

**Example using the `MyApplicationEvent` base event, above:**

```csharp
public class MyService(ITypedNoticeDispatcher dispatcher)
{
  public async Task SendImmediateNotice()
  {
    _ = await dispatcher.DispatchAsync(new ImportantNotice());
  }
  public record ImportantNotice : MyApplicationEvent;
}
```

> #### _Special Mention_ `INoticeIo`
>
> An implementation of the `INoticeIo` interface is used to actually adapt the serialized events (emitted by the typed notice dispatcher) into the I/O requests needed for your application's chosen messaging infrastructure (e.g., AWS SNS).
> (See [Notice Dispatch][notice-dispatch] for an explanation of how NiceNotice represents/adapts an I/O client.)
>
> NiceNotice does **not** include any I/O _adapters_ in the core library.
> Two implementations of `INoticeIo` are provided:
>
> * `NullNoticeIo`, the default implementation; notifications are **not** dispatched externally.
> * `CapturingNoticeIo`, simply captures emitted notifications in memory; provided to support unit testing.
>
> The [`NiceNotice.Aws.Sns` nuget package][nicenotice-aws-sdk-nuget] offers a prebuilt adapter for **Amazon Web Services Simple Notification Service**.
>
> _For other destinations (enterprise event buses)&hellip;_
>
> Your application might require a custom `INoticeIo` implementation. _Don't worry, it's a simple interface._
>
> If your event bus supports batches, then instead implement `INoticeBatchIo`. (Otherwise batches of notices are sent by executing the single-notice process repeatedly.)
>
> [Check out the `ExampleCustomDispatcher` class in the NiceNotice unit tests][example-custom-dispatcher] for an example showing how the `xUnit` test output helper was adapted to be an I/O destination for tests.
> Or check out [the `SnsNoticeIo` implementation in `NiceNotice.Aws.Sns`][nicenotice-aws-sdk-snsnoticeio] to see how AWS SNS is adapted.

### Step 4: Add Notification Types as Needed

Once the base application event is established (e.g., the `MyApplicationEvent` in the example above), you'll add additional notice types as needed to represent the messages your application sends.

It is **critical** to remember that the notification objects are **value objects** which are ultimately serialized. Use `[JsonPropertyName]` and similar `System.Text.Json` attributes to refine the serialization of your notification types.

For example, the same application which defined `MyApplicationEvent` might want to dispatch notifications upon _critical-level_ events. The application might define a `CriticalFailureDetected` event:

```csharp
public record CriticalFailureDetected : MyApplicationEvent
{
  public required string ErrorId { get; init; } = string.Empty;
  public required string Message { get; init; } = string.Empty;
  public required string Details { get; init; } = string.Empty;
}
```

Such an event would serialize like:

```json
{"errorId":"321.1","message":"Failed to communicate with the database for more than 90 seconds.","details":"Requests to database failed for configured time period (90 seconds). Considered unrecoverable.","schema":"CriticalFailureDetected","timestamp":"2025-10-11T23:43:05.3421459+00:00","id":"16e15973-1e39-4347-a3dc-3de66af0a8bc"}
```

[awssdk-sns]: https://www.nuget.org/packages/AWSSDK.SimpleNotificationService
[dotnet-generic-host]: https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host
[enterprise-event-example-custom]: https://github.com/JeremiahSanders/nicenotice/blob/98290345b0d51a3d9070afa1e118277e0e23be3f/tests/unit/ExampleEventSchemas/Custom/ExampleCustomBaseEnterpriseEvent.cs#L6-L51
[enterprise-event-example-standard]: https://github.com/JeremiahSanders/nicenotice/blob/98290345b0d51a3d9070afa1e118277e0e23be3f/tests/unit/EnterpriseEventTests.cs#L104-L126
[example-custom-dispatcher]: https://github.com/JeremiahSanders/nicenotice/blob/98290345b0d51a3d9070afa1e118277e0e23be3f/tests/unit/ExampleApplication/ExampleCustomDispatcher.cs#L5-L23
[how-typednoticedispatcher-works]: https://github.com/JeremiahSanders/nicenotice/docs/how-does-it-work.md
[Microsoft.Extensions.Hosting]: https://www.nuget.org/packages/Microsoft.Extensions.Hosting
[nicenotice-aws-sdk-docs]: https://github.com/JeremiahSanders/nicenotice-aws-sns/blob/dev/README.md
[nicenotice-aws-sdk-nuget]: https://www.nuget.org/packages/NiceNotice.Aws.Sns
[nicenotice-aws-sdk-snsnoticeio]: https://github.com/JeremiahSanders/nicenotice-aws-sns/blob/dev/src/library/SnsNoticeIo.cs
[nicenotice-nuget]: https://www.nuget.org/packages/NiceNotice
[notice-dispatch]: https://github.com/JeremiahSanders/nicenotice/tree/dev/docs/notice-dispatch.md
[notice-stream-attribute]: https://github.com/JeremiahSanders/nicenotice/tree/dev/docs/api/Jds.NiceNotice.TypedNotices/NoticeStreamAttribute.md
[service-configuration-console-example]: https://github.com/JeremiahSanders/nicenotice-aws-sns/blob/73d4bf5dbd46c6e0bafcd5bc72f8137b4e619e37/tests/example-console/Services.cs#L69-L93
[service-configuration-webapi-example]: https://github.com/JeremiahSanders/nicenotice-aws-sns/blob/73d4bf5dbd46c6e0bafcd5bc72f8137b4e619e37/tests/example-webapi/Program.cs#L20-L58
