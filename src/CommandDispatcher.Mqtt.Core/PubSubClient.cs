using CommandDispatcher.Mqtt.Interfaces;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;
using MQTTnet.Protocol;

namespace CommandDispatcher.Mqtt.Core
{
    /// <summary>
    /// Provides a facade over MqttClientBase and the underlying MqttClient.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PubSubClient<T> : MqttClientBase, IPubSubClient<T>
    {
        private readonly IMqttMessageFormatter<T> _messageFormatter;
        private readonly Dictionary<string, List<IPubSubClient<T>.MessageReceivedCallback>> _callbacks = new();

        public event Func<ConnectingFailedEventArgs, Task>? ConnectingFailed;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mqttSettings"></param>
        /// <param name="logger"></param>
        public PubSubClient(MqttSettings mqttSettings, IMqttMessageFormatter<T> messageFormatter, ILogger<PubSubClient<T>> logger) : base(mqttSettings, logger)
        {
            _messageFormatter = messageFormatter;
            StartAsync().Wait();
        }

        /// <summary>
        /// Performs the necessary setup to initialize and start the client.
        /// </summary>
        /// <returns></returns>
        protected override async Task StartAsync()
        {
            await base.StartAsync();
            _mqttClient!.ApplicationMessageReceivedAsync += MessageReceivedHandler;
        }

        /// <summary>
        /// Publishes the message to the specified topic.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="payload"></param>
        public async Task Publish(string topic, T payload, bool isRetained = false)
        {
            Guard.IsNull(topic);
            Guard.IsNull(payload);

            byte[] formattedMessage = _messageFormatter.EncodeMessage(payload, out var contentType);

            if (formattedMessage is null || formattedMessage.Length == 0)
            {
                _logger.LogWarning("Payload is empty or null.");
                return;
            }

            var qos = (MqttQualityOfServiceLevel)_mqttSettings!.MqttQualityOfServiceLevel;
            MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(formattedMessage)
                .WithQualityOfServiceLevel(qos)
                .WithRetainFlag(isRetained)
                .Build();

            _logger.LogInformation("Publishing message: {payload}", payload);
            await _mqttClient!.EnqueueAsync(message);
        }

        /// <summary>
        /// Clears the current retained message for the specified topic.
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public async Task ClearRetainedMessage(string topic)
        {
            Guard.IsNull(topic);

            byte[] zeroBytes = [];
            var qos = (MqttQualityOfServiceLevel)_mqttSettings!.MqttQualityOfServiceLevel;
            MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(zeroBytes)
                .WithQualityOfServiceLevel(qos)
                .WithRetainFlag(true)
                .Build();

            _logger.LogInformation("Clearing retained message on topic: {topic}", topic);
            await _mqttClient!.EnqueueAsync(message);
        }

        /// <summary>
        /// Subscribes to the specified topic and registers a typed callback to be invoked when a message is received.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async Task Subscribe(string topic, IPubSubClient<T>.MessageReceivedCallback callback)
        {
            Guard.IsNull(topic);
            Guard.IsNull(callback);

            MqttTopicFilter topicFilter = new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .WithAtLeastOnceQoS()
                .Build();

            if (_callbacks.ContainsKey(topic))
            {
                _callbacks[topic].Add(callback);
            }
            else
            {
                _callbacks.Add(topic, [callback]);
            }

            _logger.LogInformation("Subscribing to {topic}", topic);
            await _mqttClient!.SubscribeAsync(new[] { topicFilter });
        }

        /// <summary>
        /// Unsubscribes from the specified topic.
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public async Task UnSubscribe(string topic)
        {
            Guard.IsNull(topic);

            _callbacks.Remove(topic);
            await _mqttClient!.UnsubscribeAsync(topic);
        }

        protected Task MessageReceivedHandler(MqttApplicationMessageReceivedEventArgs arg)
        {
            string topic = arg.ApplicationMessage.Topic;
            var messageBytes = arg.ApplicationMessage.PayloadSegment.Array;

            if (messageBytes is null || messageBytes.Length == 0)
            {
                _logger.LogWarning("Payload is empty or null.");
                return Task.CompletedTask;
            }
            try
            {
                var message = _messageFormatter.DecodeMessage(messageBytes);

                if (_callbacks.Count != 0)
                {
                    _logger.LogDebug("Received message: {payload} on {topic}.", message, topic);

                    if (_callbacks.TryGetValue(topic, out var callbackList))
                    {
                        foreach (IPubSubClient<T>.MessageReceivedCallback callback in callbackList)
                        {
                            callback(topic, message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Unable to deserialize payload: {ex}.", ex);
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        protected override Task MqttClient_ConnectingFailedAsync(ConnectingFailedEventArgs arg)
        {
            base.MqttClient_ConnectingFailedAsync(arg);
            return Task.Run(() => ConnectingFailed?.Invoke(arg));
        }
    }

    /// <summary>
    /// Provides a facade over MqttClientBase and the underlying MqttClient.
    /// </summary>
    public class PubSubClient : MqttClientBase, IPubSubClient
    {
        private readonly Dictionary<string, IPubSubClient.MessageReceivedCallback> _callbacks = new();
        public event Func<ConnectingFailedEventArgs, Task>? ConnectingFailed;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mqttSettings"></param>
        /// <param name="logger"></param>
        public PubSubClient(MqttSettings mqttSettings, ILogger logger) : base(mqttSettings, logger)
        {
            StartAsync().Wait();
        }

        /// <summary>
        /// Performs the necessary setup to initialize and start the client.
        /// </summary>
        /// <returns></returns>
        protected override async Task StartAsync()
        {
            await base.StartAsync();
            _mqttClient!.ApplicationMessageReceivedAsync += MessageReceivedHandler;

        }

        /// <summary>
        /// Publishes the message to the specified topic.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="payload"></param>
        /// <exception cref="NotImplementedException"></exception>
        public async Task Publish(string topic, string payload, bool isRetained = false)
        {
            Guard.IsNull(topic);
            Guard.IsNull(payload);

            var qos = (MqttQualityOfServiceLevel)_mqttSettings!.MqttQualityOfServiceLevel;
            MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(qos)
                .WithRetainFlag(isRetained)
                .Build();

            _logger.LogInformation("Publishing message: {payload}", payload);
            await _mqttClient!.EnqueueAsync(message);
        }

        /// <summary>
        /// Clears the current retained message for the specified topic.
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public async Task ClearRetainedMessage(string topic)
        {
            byte[] zeroBytes = [];
            var qos = (MqttQualityOfServiceLevel)_mqttSettings!.MqttQualityOfServiceLevel;
            MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(zeroBytes)
                .WithQualityOfServiceLevel(qos)
                .WithRetainFlag(true)
                .Build();

            _logger.LogInformation("Clearing retained message on topic: {topic}", topic);
            await _mqttClient!.EnqueueAsync(message);
        }

        /// <summary>
        /// Subscribes to the specified topic and registers a json string callback to be invoked when a message is received.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async Task Subscribe(string topic, IPubSubClient.MessageReceivedCallback callback)
        {
            Guard.IsNull(topic);
            Guard.IsNull(callback);

            MqttTopicFilter topicFilter = new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .WithAtLeastOnceQoS()
                .Build();

            _callbacks.Add(topic, callback);

            _logger.LogInformation("Subscribing to {topic}", topic);
            await _mqttClient!.SubscribeAsync(new[] { topicFilter });
        }

        /// <summary>
        /// Unsubscribes from the specified topic.
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public async Task UnSubscribe(string topic)
        {
            Guard.IsNull(topic);

            _callbacks.Remove(topic);
            await _mqttClient!.UnsubscribeAsync(topic);
        }

        protected Task MessageReceivedHandler(MqttApplicationMessageReceivedEventArgs arg)
        {
            try
            {
                string topic = arg.ApplicationMessage.Topic;
                string payload = arg.ApplicationMessage.ConvertPayloadToString();

                _logger.LogDebug("Received message: {payload} on {topic}.", payload, topic);

                if (_callbacks.Any())
                {
                    if (!_callbacks.TryGetValue(topic, out var callback))
                    {
                        return Task.CompletedTask;
                    }
                    callback(topic, payload);

                    return Task.CompletedTask;
                }

                return Task.CompletedTask;

            }
            catch (Exception ex)
            {
                _logger.LogError("{exception}", ex.ToString());
                return Task.CompletedTask;
            }
        }

        protected override Task MqttClient_ConnectingFailedAsync(ConnectingFailedEventArgs arg)
        {
            base.MqttClient_ConnectingFailedAsync(arg);
            return Task.Run(() => ConnectingFailed?.Invoke(arg));
        }
    }
}
