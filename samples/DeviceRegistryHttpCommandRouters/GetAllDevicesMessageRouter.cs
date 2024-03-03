using Azure.Messaging;
using CommandDispatcher.Mqtt.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DeviceRegistryCommandRouters;

public class GetAllDevicesCommandRouter : DeviceRegistryCommandRouter, ICommandRouter<CloudEvent>
{
    public GetAllDevicesCommandRouter(IHttpClientFactory httpClientFactory, ILogger<GetAllDevicesCommandRouter> logger, IConfiguration configuration) :
        base(httpClientFactory, logger, configuration)
    { }

    Predicate<CloudEvent> ICommandRouter<CloudEvent>.MessageSelector =>
        message => message.Type == DeviceRegistryMessageTypes.GetAllDevices.ToString();

    protected override async Task<string> CallHttpEndpoint(HttpClient httpClient, CloudEvent message)
    {
        using HttpResponseMessage response = await httpClient.GetAsync(_deviceRegistryEndpoint);
        return await response.Content.ReadAsStringAsync();
    }
}

