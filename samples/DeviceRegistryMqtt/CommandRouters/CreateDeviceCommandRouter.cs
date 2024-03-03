using Azure.Messaging;
using CommandDispatcher.Mqtt.CloudEvents;
using CommandDispatcher.Mqtt.Interfaces;
using DeviceRegistryMqtt.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DeviceRegistryMqtt.CommandRouters
{
    public class CreateDeviceCommandRouter : ICommandRouter<CloudEvent>
    {
        private readonly IDeviceRegistryService _registryService;
        private readonly ILogger<CreateDeviceCommandRouter> _logger;
        private readonly string _incomingTopic;
        private readonly string _outgoingTopic;

        public CreateDeviceCommandRouter(IDeviceRegistryService registryService, ILogger<CreateDeviceCommandRouter> logger, IConfiguration configuration)
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
            if (message.Data is null)
            {
                _logger.LogWarning("No device data sent with message");
                return;
            }
            try
            {
                var device = JsonSerializer.Deserialize<Device>(message.Data);
                if (device is null)
                {
                    _logger.LogWarning("Unable to deseriliaze device data");
                    return;
                }
                _logger.LogInformation("Creating device: {device}", device);
                var createdDevice = await _registryService.CreateDevice(device);
                var response = new CloudEvent("DeviceRegistryService", message.Type, createdDevice);
                response.SetCorrelationId(message.GetCorrelationId());

                _logger.LogInformation("Publishing response to topic {topic}", _outgoingTopic);
                await PubSubClient!.Publish(_outgoingTopic, response);
            }
            catch (JsonException ex)
            {
                _logger.LogError("Unable to deseriliaze device data: {}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while processing the message: {}", ex.Message);
            }
        }
    }
}
