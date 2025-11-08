# Notice Dispatch (how the messages are sent)

At the core of NiceNotice are [the `INoticeIo` interface][INoticeIo] and [the `INoticeBatchIo` interface][INoticeBatchIo]. These are the central abstractions which represent your I/O infrastructure. The default `ITypedNoticeDispatcher` uses the `INoticeIo` (or `INoticeBatchIo`) implementation to dispatch serialized notifications.

`INoticeIo` exposes a single method: `DispatchAsync()`.

```csharp
Task<string> DispatchAsync(EventStreamId stream, string notice, CancellationToken cancellationToken = default);
```

The `INoticeIo` interface is intended to function as a lowest-common-denominator representation of common message bus interfaces. (See inset below showing methods in common message bus client APIs.)

At the core of `DispatchAsync` are two pieces of information which are **central** to the workflow:

* Where is the notice going? `EventStreamId stream`
* What is the message? `string notice`

`INoticeBatchIo` **extends** `INoticeIo`, adding a single method: `DispatchNoticesAsync()`

```csharp
Task<BatchIoNoticeDispatchResult> DispatchNoticesAsync(
    IReadOnlyDictionary<string, BatchedIoRequestNotice> notices, 
    BatchDispatchOptions? batchDispatchOptions = null, 
    CancellationToken cancellationToken = default)
```

There is some added complexity with batch dispatch; notably, the need to identify the notices within the batch so that their respective success/failure can be communicated in the response. Beyond that, it's built on the same simple `EventStreamId` and `string` premise.

The `INoticeIo` interface **needs to be implemented in your project** _**or**_ by using a prebuilt NiceNotice I/O adapter, such as [the `NiceNotice.Aws.Sns` adapter for AWS SNS][nicenotice-aws-sns-package].

> **Message bus interfaces**
>
> These examples help explain the design rational behind the `INoticeIo` abstraction. As you can see, common event buses generally have some form of routing information and then a `string` or `byte[]` message payload.
>
> [AWS SNS: `IAmazonSimpleNotificationService.PublishAsync`][sns-publishasync]
>
> ```csharp
> Task<PublishResponse> PublishAsync(string topicArn, string message, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
> ```
>
> [RabbitMQ: `IChannel.BasicPublishAsync`][rabbitmq-basicpublishasync]
>
> ```csharp
> ValueTask BasicPublishAsync<TProperties>(string exchange, string routingKey,
>     bool mandatory, TProperties basicProperties, ReadOnlyMemory<byte> body,
>     CancellationToken cancellationToken = default)
>     where TProperties : IReadOnlyBasicProperties, IAmqpHeader;
> ```

[INoticeIo]: https://github.com/JeremiahSanders/nicenotice/blob/dev/docs/api/Jds.NiceNotice/INoticeIo.md
[INoticeBatchIo]: https://github.com/JeremiahSanders/nicenotice/blob/dev/docs/api/Jds.NiceNotice/INoticeBatchIo.md
[nicenotice-aws-sns-package]: https://www.nuget.org/packages/NiceNotice.Aws.Sns
[rabbitmq-basicpublishasync]: https://github.com/rabbitmq/rabbitmq-dotnet-client/blob/8698fd6e72c12453612262d1d64e9ed9b244b8f8/projects/RabbitMQ.Client/IChannel.cs#L209C1-L212C71
[sns-publishasync]: https://github.com/aws/aws-sdk-net/blob/cf2f0787d8149dfcfced66968bf713d5468f72b5/sdk/src/Services/SimpleNotificationService/Generated/_netstandard/IAmazonSimpleNotificationService.cs#L1788C9-L1788C160
