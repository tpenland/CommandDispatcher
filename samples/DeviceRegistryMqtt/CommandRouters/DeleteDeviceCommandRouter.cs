using Azure.Messaging;
using CommandDispatcher.Mqtt.CloudEvents;
using CommandDispatcher.Mqtt.Interfaces;
using DeviceRegistryMqtt.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DeviceRegistryMqtt.CommandRouters
{
    internal class DeleteDeviceCommandRouter : ICommandRouter<CloudEvent>
    {
        private readonly IDeviceRegistryService _registryService;
        private readonly ILogger<DeleteDeviceCommandRouter> _logger;
        private readonly string _incomingTopic;
        private readonly string _outgoingTopic;

        public DeleteDeviceCommandRouter(IDeviceRegistryService registryService, ILogger<DeleteDeviceCommandRouter> logger, IConfiguration configuration)
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
            message => message.Type == DeviceRegistryCommandTypes.DeleteDevice.ToString();

        public IPubSubClient<CloudEvent>? PubSubClient { get; set; }

        public string IncomingTopic => _incomingTopic;

        public string? OutgoingTopic => _outgoingTopic;

        public async Task RouteAsync(CloudEvent message)
        {
            try
            {
                if (message.Data is null)
                {
                    _logger.LogWarning("No device data sent with message");
                    return;
                }
                _logger.LogInformation("Deleting device {id}", message.Data);
                
                var id = Cleanup(message.Data.ToString());

                await _registryService.DeleteDevice(id);

                var response = new CloudEvent("DeviceRegistryService", message.Type, id);
                response.SetCorrelationId(message.GetCorrelationId());

                _logger.LogInformation("Publishing response to topic {topic}", _outgoingTopic);
                await PubSubClient!.Publish(_outgoingTopic, response);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while processing the message: {}", ex.Message);
            }
        }

        private string Cleanup(string data)
        {
            data = Regex.Unescape(data);
            data = data.Replace("\"", "");
            return data;
        }

    }
}
