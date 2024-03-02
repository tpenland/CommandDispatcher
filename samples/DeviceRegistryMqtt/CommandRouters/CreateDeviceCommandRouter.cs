using Azure.Messaging;
using CommandDispatcher.Mqtt.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DeviceRegistryMqtt.CommandRouters
{
    public class CreateDeviceCommandRouter : ICommandRouter<CloudEvent>
    {
        private readonly IDeviceRegistryService _deviceRegistryService;
        private readonly ILogger<CreateDeviceCommandRouter> _logger;
        private readonly string _incomingTopic;
        private readonly string? _outgoingTopic;
        const string NullContentMessage = "Received message with no content";
        const string InvalidContentMessage = "Received message with invalid content";

        public CreateDeviceCommandRouter(IDeviceRegistryService deviceRegistryService, ILogger<CreateDeviceCommandRouter> logger, IConfiguration configuration)
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
            message => message.Type == DeviceRegistryCommandTypes.CreateDevice.ToString();

        public IPubSubClient<CloudEvent>? PubSubClient { get; set; }

        public string IncomingTopic => _incomingTopic;

        public string? OutgoingTopic => _outgoingTopic;

        public async Task RouteAsync(CloudEvent message)
        {
            _logger.LogInformation("Received message {message}", message.Data);

            try
            {
#pragma warning disable CS8604 // Possible null reference argument. A message with no data here is an exception case.
                var device = JsonSerializer.Deserialize<Device>(message.Data?.ToString());
#pragma warning restore CS8604 // Possible null reference argument.
                var createdDevice = await _deviceRegistryService.CreateDevice(device!);
                await this.SendResponseMessage(message, JsonSerializer.Serialize(createdDevice));
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning("Received message with no content: {}", ex.Message);
                await this.SendResponseMessage(message, NullContentMessage);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning("Received message with invalid content: {}", ex.Message);
                await this.SendResponseMessage(message, InvalidContentMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while processing the message: {}", ex.Message);
                throw;
            }
        }
    }
}
