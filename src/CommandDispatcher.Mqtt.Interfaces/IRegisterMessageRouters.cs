using Microsoft.Extensions.DependencyInjection;

namespace CommandDispatcher.Mqtt.Interfaces
{
    public interface IRegisterCommandRouters
    {
        void RegisterCommandRouters(IServiceCollection services);
    }
}
