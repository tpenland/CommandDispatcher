using CloudNative.CloudEvents;
using CommandDispatcher.Mqtt.CloudEvents;
using CommandDispatcher.Mqtt.Interfaces;
using Microsoft.Extensions.Logging;

namespace CommandDispatcher.Mqtt.Dispatcher.ConsoleHost
{
    // This will always be registered and can be used to verify the ConsoleHost is running.
    internal class LoopbackCommandRouter : ICommandRouter<CloudEvent>
    {
        private readonly ILogger<LoopbackCommandRouter> _logger;
        public LoopbackCommandRouter(ILogger<LoopbackCommandRouter> logger)
        {
            _logger = logger;
        }
        
        public IPubSubClient<CloudEvent>? PubSubClient { get; set; }
        
        public string IncomingTopic => "loopback/input";

        public string OutgoingTopic => "loopback/output";

        Predicate<CloudEvent> ICommandRouter<CloudEvent>.MessageSelector => _ => true;

        public async Task RouteAsync(CloudEvent message)
        {
            await Task.Delay(100);
            _logger.LogInformation("Received message {message}", message.Data);

            var response = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Source = new Uri("urn:CommandDispatcher:loopback:response"),
                Type = "CommandDispatcher.loopback.response",
                Subject = "loopback",
                Data = message.Data,
                Time = DateTime.UtcNow
            };
            response.SetCorrelationId(message.GetCorrelationId());

            if (PubSubClient != null)
            {
                await PubSubClient.Publish(OutgoingTopic, response);
            }
        }
    }
}
