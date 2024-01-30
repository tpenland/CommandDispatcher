using Imv.Messaging.Mqtt.Interfaces;
using Imv.Messaging.Mqtt.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DeviceRegistryCommandRouters;

public class GetDeviceCommandRouter : DeviceRegistryCommandRouter, ICommandRouter<MessageEnvelope>
{
    public GetDeviceCommandRouter(IHttpClientFactory httpClientFactory, ILogger<GetDeviceCommandRouter> logger, IConfiguration configuration) :
        base(httpClientFactory, logger, configuration)
    { }

    Predicate<MessageEnvelope> ICommandRouter<MessageEnvelope>.MessageSelector =>
        message => message.MessageType == DeviceRegistryMessageTypes.GetDevice.ToString();

    protected override async Task<string> CallHttpEndpoint(HttpClient httpClient, MessageEnvelope message)
    {
        using HttpResponseMessage response = await httpClient.GetAsync($"{_deviceRegistryEndpoint}{message.Payload}");
        return await response.Content.ReadAsStringAsync();
    }
}

