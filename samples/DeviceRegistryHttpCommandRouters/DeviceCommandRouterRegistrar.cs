using Azure.Messaging;
using CommandDispatcher.Mqtt.Interfaces;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceRegistryCommandRouters
{
    public class DeviceCommandRouterRegistrar : IRegisterCommandRouters
    {
        public void RegisterCommandRouters(IServiceCollection services)
        {
            Guard.IsNotNull(services);

            services.AddHttpClient();
            services.AddSingleton<ICommandRouter<CloudEvent>, CreateDeviceCommandRouter>();
            services.AddSingleton<ICommandRouter<CloudEvent>, GetDeviceCommandRouter>();
            services.AddSingleton<ICommandRouter<CloudEvent>, GetAllDevicesCommandRouter>();
        }
    }
}
