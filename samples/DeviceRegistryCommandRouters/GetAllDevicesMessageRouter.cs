using Imv.Messaging.Mqtt.Interfaces;
using Imv.Messaging.Mqtt.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DeviceRegistryCommandRouters;

public class GetAllDevicesCommandRouter : DeviceRegistryCommandRouter, ICommandRouter<MessageEnvelope>
{
    public GetAllDevicesCommandRouter(IHttpClientFactory httpClientFactory, ILogger<GetAllDevicesCommandRouter> logger, IConfiguration configuration) :
        base(httpClientFactory, logger, configuration)
    { }

    Predicate<MessageEnvelope> ICommandRouter<MessageEnvelope>.MessageSelector =>
        message => message.MessageType == DeviceRegistryMessageTypes.GetAllDevices.ToString();

    protected override async Task<string> CallHttpEndpoint(HttpClient httpClient, MessageEnvelope message)
    {
        using HttpResponseMessage response = await httpClient.GetAsync(_deviceRegistryEndpoint);
        return await response.Content.ReadAsStringAsync();
    }
}

