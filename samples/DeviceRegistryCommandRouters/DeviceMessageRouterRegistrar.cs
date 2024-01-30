using CommunityToolkit.Diagnostics;
using Imv.Messaging.Mqtt.Interfaces;
using Imv.Messaging.Mqtt.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceRegistryCommandRouters
{
    public class DeviceCommandRouterRegistrar : IRegisterCommandRouters
    {
        public void RegisterCommandRouters(IServiceCollection services)
        {
            Guard.IsNotNull(services);

            services.AddHttpClient();
            services.AddSingleton<ICommandRouter<MessageEnvelope>, CreateDeviceCommandRouter>();
            services.AddSingleton<ICommandRouter<MessageEnvelope>, GetDeviceCommandRouter>();
            services.AddSingleton<ICommandRouter<MessageEnvelope>, GetAllDevicesCommandRouter>();
        }
    }
}
