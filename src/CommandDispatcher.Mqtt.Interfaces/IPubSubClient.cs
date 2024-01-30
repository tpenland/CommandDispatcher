namespace CommandDispatcher.Mqtt.Interfaces
{
    public interface IPubSubClient
    {
        /// <summary>
        /// Callback for when a message is received.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="messageJson"></param>
        /// <returns></returns>
        public delegate Task MessageReceivedCallback(string topic, string messageJson);
        /// <summary>
        /// Subscribes to the specified topic and registers the callback to be invoked when a message is received.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        Task Subscribe(string topic, MessageReceivedCallback callback);
        /// <summary>
        /// Unsubscribes from the specified topic.
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        Task UnSubscribe(string topic);
        /// <summary>
        /// Publishes a message to the specified topic.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <param name="payload"></param>
        /// <param name="isRetained"></param>
        Task Publish(string topic, string payload, bool isRetained = false);
        /// <summary>
        /// Clears the current retained message for the specified topic.
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        Task ClearRetainedMessage(string topic);
    }

    public interface IPubSubClient<T>
    {
        /// <summary>
        /// Callback for when a message is received.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public delegate Task MessageReceivedCallback(string topic, T message);
        /// <summary>
        /// Subscribes to the specified topic and registers the callback to be invoked when a message is received.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        Task Subscribe(string topic, MessageReceivedCallback callback);
        /// <summary>
        /// Unsubscribes from the specified topic.
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        Task UnSubscribe(string topic);
        /// <summary>
        /// Publishes a message to the specified topic.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <param name="payload"></param>
        /// <param name="isRetained"></param>
        Task Publish(string topic, T payload, bool isRetained = false);
        /// <summary>
        /// Clears the current retained message for the specified topic.
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        Task ClearRetainedMessage(string topic);
    }
}
