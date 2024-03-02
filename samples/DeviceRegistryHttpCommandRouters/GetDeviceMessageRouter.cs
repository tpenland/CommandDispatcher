using CommandDispatcher.Mqtt.Interfaces;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DeviceRegistryCommandRouters;

public class GetDeviceCommandRouter : DeviceRegistryCommandRouter, ICommandRouter<CloudEvent>
{
    public GetDeviceCommandRouter(IHttpClientFactory httpClientFactory, ILogger<GetDeviceCommandRouter> logger, IConfiguration configuration) :
        base(httpClientFactory, logger, configuration)
    { }

    Predicate<CloudEvent> ICommandRouter<CloudEvent>.MessageSelector =>
        message => message.Type == DeviceRegistryMessageTypes.GetDevice.ToString();

    protected override async Task<string> CallHttpEndpoint(HttpClient httpClient, CloudEvent message)
    {
        using HttpResponseMessage response = await httpClient.GetAsync($"{_deviceRegistryEndpoint}{message.Data}");
        return await response.Content.ReadAsStringAsync();
    }
}

