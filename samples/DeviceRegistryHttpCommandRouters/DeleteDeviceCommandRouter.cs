using Azure.Messaging;
using CommandDispatcher.Mqtt.Interfaces;
using DeviceRegistryCommandRouters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DeviceRegistryHttpCommandRouters
{
    public class DeleteDeviceCommandRouter : DeviceRegistryCommandRouter, ICommandRouter<CloudEvent>
    {
        public DeleteDeviceCommandRouter(IHttpClientFactory httpClientFactory, ILogger<GetDeviceCommandRouter> logger, IConfiguration configuration) :
            base(httpClientFactory, logger, configuration)
        { }

        Predicate<CloudEvent> ICommandRouter<CloudEvent>.MessageSelector =>
            message => message.Type == DeviceRegistryCommandTypes.DeleteDevice.ToString();

        protected override async Task<string> CallHttpEndpoint(HttpClient httpClient, CloudEvent message)
        {
            using HttpResponseMessage response = await httpClient.DeleteAsync($"{_deviceRegistryEndpoint}{message.Data}");
            return await response.Content.ReadAsStringAsync();
        }
    }
}