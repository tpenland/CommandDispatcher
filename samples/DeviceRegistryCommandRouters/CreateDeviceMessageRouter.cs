using Imv.Messaging.Mqtt.Interfaces;
using Imv.Messaging.Mqtt.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DeviceRegistryCommandRouters;

public class CreateDeviceCommandRouter : DeviceRegistryCommandRouter, ICommandRouter<MessageEnvelope>
{
    public CreateDeviceCommandRouter(IHttpClientFactory httpClientFactory, ILogger<CreateDeviceCommandRouter> logger, IConfiguration configuration):
        base(httpClientFactory, logger, configuration)
    { }

    Predicate<MessageEnvelope> ICommandRouter<MessageEnvelope>.MessageSelector =>
        message => message.MessageType == DeviceRegistryMessageTypes.CreateDevice.ToString();

    protected override async Task<string> CallHttpEndpoint(HttpClient httpClient, MessageEnvelope message)
    {
        using StringContent jsonContent = new(message.Payload, Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await httpClient.PostAsync(_deviceRegistryEndpoint, jsonContent);

        return await response.Content.ReadAsStringAsync();
    }
}

