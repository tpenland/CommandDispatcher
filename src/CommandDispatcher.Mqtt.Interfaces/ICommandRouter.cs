namespace CommandDispatcher.Mqtt.Interfaces;

/// <summary>
/// Interface for a message router. A message router is responsible for routing a message to the correct handler.
/// The receiving handler can be out of process or in process.
/// </summary>
public interface ICommandRouter<T>
{
    /// <summary>
    /// Predicate for selecting the correct message.
    /// https://learn.microsoft.com/en-us/dotnet/api/system.predicate-1?view=net-7.0
    /// This is to be used in fine grained routing scenarios where a single topic is used for many different messages.
    /// For example, to filter based on message type:
    /// Predicate<T> MessageSelector { get; } => message => message.MessageType == "MyMessageType";
    /// Alternatively, if no further filtering is required, then this can be set to return true for all messages:
    /// Predicate<T> MessageSelector { get; } => _ => true;
    /// </summary>
    Predicate<T> MessageSelector { get; }
    /// <summary>
    /// The publisher that will be used to publish any responses to the received message.
    /// This will be injected by the CommandDispatcher at runtime.
    /// </summary>
    IPubSubClient<T>? PubSubClient { get; set; }
    /// <summary>
    /// Route a message to the correct handler. This will be called by the CommandDispatcher at runtime.
    /// The intent is this can handle whatever is necessary to get the message to the correct handler, receive
    /// the response, and then publish the response and any other status messages using the MessagePublisher.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <returns></returns>
    Task RouteAsync(T message);
    /// <summary>
    /// The topic that will be subscribed to for incoming messages. 
    /// This is used by the CommandDispatcher to subscribe to the correct topic.
    /// </summary>
    string IncomingTopic { get; }
    /// <summary>
    /// The topic to which outgoing messages will be published.
    /// This is optional and is only used at the discretion of the implementor if the message 
    /// router needs to publish a response.
    /// </summary>
    string? OutgoingTopic { get; }
}