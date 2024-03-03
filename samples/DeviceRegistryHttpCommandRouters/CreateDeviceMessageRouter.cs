using Azure.Messaging;
using CommandDispatcher.Mqtt.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DeviceRegistryCommandRouters;

public class CreateDeviceCommandRouter : DeviceRegistryCommandRouter, ICommandRouter<CloudEvent>
{
    public CreateDeviceCommandRouter(IHttpClientFactory httpClientFactory, ILogger<CreateDeviceCommandRouter> logger, IConfiguration configuration):
        base(httpClientFactory, logger, configuration)
    { }

    Predicate<CloudEvent> ICommandRouter<CloudEvent>.MessageSelector =>
        message => message.Type == DeviceRegistryMessageTypes.CreateDevice.ToString();

    protected override async Task<string> CallHttpEndpoint(HttpClient httpClient, CloudEvent message)
    {
        string? content = message?.Data?.ToString();
        if (content is null) return string.Empty;

        using StringContent jsonContent = new(content, Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await httpClient.PostAsync(_deviceRegistryEndpoint, jsonContent);

        return await response.Content.ReadAsStringAsync();
    }
}

