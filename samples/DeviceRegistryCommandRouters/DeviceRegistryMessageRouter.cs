using Imv.Messaging.Mqtt.Interfaces;
using Imv.Messaging.Mqtt.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace DeviceRegistryCommandRouters
{
    public abstract class DeviceRegistryCommandRouter : ICommandRouter<MessageEnvelope>
    {
        protected readonly ILogger _logger;
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly string _deviceRegistryEndpoint;
        protected readonly Uri _baseAddress;
        protected readonly string _incomingTopic;
        protected readonly string _outgoingTopic;

        protected DeviceRegistryCommandRouter(IHttpClientFactory httpClientFactory, ILogger logger, IConfiguration configuration)
        {
            (_httpClientFactory, _logger) = (httpClientFactory, logger);

            ArgumentException.ThrowIfNullOrEmpty(configuration["DeviceRegistry:BaseAddress"]);
            ArgumentException.ThrowIfNullOrEmpty(configuration["DeviceRegistry:ApiEndpoint"]);
            ArgumentException.ThrowIfNullOrEmpty(configuration["DeviceRegistry:IncomingTopic"]);
            ArgumentException.ThrowIfNullOrEmpty(configuration["DeviceRegistry:OutgoingTopic"]);
            _baseAddress = new Uri(configuration["DeviceRegistry:BaseAddress"]!);
            _deviceRegistryEndpoint = configuration["DeviceRegistry:ApiEndpoint"]!;
            _incomingTopic = configuration["DeviceRegistry:IncomingTopic"]!;
            _outgoingTopic = configuration["DeviceRegistry:OutgoingTopic"]!;
        }

        public string IncomingTopic => _incomingTopic;

        public string? OutgoingTopic => _outgoingTopic;

        public IPubSubClient<MessageEnvelope>? PubSubClient { get; set; }

        public virtual Predicate<MessageEnvelope> MessageSelector => throw new NotImplementedException();

        public virtual async Task RouteAsync(MessageEnvelope message)
        {
            _logger.LogInformation("Received message {message}", message.Payload);

            HttpClient httpClient = GetClient();
            string jsonResponse = await CallHttpEndpoint(httpClient, message);
            await PublishResponse(jsonResponse, message);
        }
        protected virtual HttpClient GetClient()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = _baseAddress;
            return httpClient;
        }

        protected abstract Task<string> CallHttpEndpoint(HttpClient httpClient, MessageEnvelope message);

        protected virtual async Task PublishResponse(string jsonResponse, MessageEnvelope message)
        {
            byte[] bytes = Encoding.Default.GetBytes(jsonResponse);
            jsonResponse = Encoding.UTF8.GetString(bytes);

            var responseMessage = new MessageEnvelope
            {
                Id = Guid.NewGuid().ToString(),
                CorrelationId = message.CorrelationId,
                ContextId = message.ContextId,
                MessageType = message.MessageType,
                Payload = jsonResponse,
                CreatedAt = DateTime.UtcNow,
            };

            if (PubSubClient != null)
            {
                await PubSubClient.Publish(OutgoingTopic!, responseMessage);
            }
        }
    }
}
