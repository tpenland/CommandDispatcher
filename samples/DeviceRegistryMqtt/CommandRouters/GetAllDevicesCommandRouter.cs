using Azure.Messaging;
using CommandDispatcher.Mqtt.CloudEvents;
using CommandDispatcher.Mqtt.Interfaces;
using DeviceRegistryMqtt.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DeviceRegistryMqtt.CommandRouters
{
    public class GetAllDevicesCommandRouter : ICommandRouter<CloudEvent>
    {
        private readonly IDeviceRegistryService _registryService;
        private readonly ILogger<GetAllDevicesCommandRouter> _logger;
        private readonly string _incomingTopic;
        private readonly string _outgoingTopic;

        public GetAllDevicesCommandRouter(IDeviceRegistryService registryService, ILogger<GetAllDevicesCommandRouter> logger, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(registryService);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentException.ThrowIfNullOrEmpty(configuration["DeviceRegistry:IncomingTopic"]);
            ArgumentException.ThrowIfNullOrEmpty(configuration["DeviceRegistry:OutgoingTopic"]);

            (_registryService, _logger) = (registryService, logger);
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
            try
            {
                _logger.LogInformation("Getting all devices");
                var devices = await _registryService.GetAllDevices();
                var response = new CloudEvent("DeviceRegistryService", message.Type, devices);
                response.SetCorrelationId(message.GetCorrelationId());
                
                _logger.LogInformation("Publishing response to topic {topic}", _outgoingTopic);
                await PubSubClient!.Publish(_outgoingTopic, response);
            }
            catch (Exception ex) 
            {
                _logger.LogError("An error occurred while processing the message: {}", ex.Message);
            }
        }
    }
}
