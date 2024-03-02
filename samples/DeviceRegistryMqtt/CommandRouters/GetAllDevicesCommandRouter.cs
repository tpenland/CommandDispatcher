using Azure.Messaging;
using CommandDispatcher.Mqtt.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DeviceRegistryMqtt.CommandRouters
{
    public class GetAllDevicesCommandRouter : ICommandRouter<CloudEvent>
    {
        private readonly IDeviceRegistryService _deviceRegistryService;
        private readonly ILogger<GetAllDevicesCommandRouter> _logger;
        private readonly string _incomingTopic;
        private readonly string? _outgoingTopic;

        public GetAllDevicesCommandRouter(IDeviceRegistryService deviceRegistryService, ILogger<GetAllDevicesCommandRouter> logger, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(deviceRegistryService);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentException.ThrowIfNullOrEmpty(configuration["DeviceRegistry:IncomingTopic"]);
            ArgumentException.ThrowIfNullOrEmpty(configuration["DeviceRegistry:OutgoingTopic"]);
            _deviceRegistryService = deviceRegistryService;
            _logger = logger;
            _incomingTopic = configuration["DeviceRegistry:IncomingTopic"]!;
            _outgoingTopic = configuration["DeviceRegistry:OutgoingTopic"]!;
        }

        public Predicate<CloudEvent> MessageSelector =>
            message => message.Type == DeviceRegistryCommandTypes.GetAllDevices.ToString();

        public IPubSubClient<CloudEvent>? PubSubClient { get; set; }

        public string IncomingTopic => _incomingTopic;

        public string? OutgoingTopic => _outgoingTopic;

        public async Task RouteAsync(CloudEvent message)
        {
            _logger.LogInformation("Received message {message}", message.Data);

            try
            {
                var devices = await _deviceRegistryService.GetAllDevices();
                await this.SendResponseMessage(message, JsonSerializer.Serialize(devices));
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while processing the message: {}", ex.Message);
                throw;
            }
        }
    }
}
