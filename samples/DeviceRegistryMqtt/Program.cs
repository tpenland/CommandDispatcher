using Azure.Messaging;
using CommandDispatcher.Mqtt.Core;
using CommandDispatcher.Mqtt.Interfaces;
using DeviceRegistryMqtt.CommandRouters;
using DeviceRegistryMqtt.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DeviceRegistryMqtt
{
    internal static class Program
    {
        private const string Mqtt_Settings_Node = "MqttSettings";
        private static ILogger? _logger;

        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder();

            // Need logger for this code since this cannot be injected
            InitLoggingForProgram();
            // Register all services need to run the Dispatcher
            RegisterServices(builder);

            var host = builder.Build();

            _ = host.Services.GetRequiredService<CommandDispatcher<CloudEvent>>();

            await host.WaitForShutdownAsync();
        }

        private static void InitLoggingForProgram()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddConsole();
                builder.AddEventSourceLogger();
            });
            _logger = loggerFactory.CreateLogger("Program");
        }

        private static void RegisterServices(HostApplicationBuilder builder)
        {
            var mqttSettings = LoadMqttSettings(builder.Configuration);

            builder.Services.AddSingleton<IDeviceRepository, DeviceRepository>();
            builder.Services.AddSingleton<IDeviceRegistryService, DeviceRegistryService>();
            builder.Services.AddSingleton<ICommandRouter<CloudEvent>, CreateDeviceCommandRouter>();
            builder.Services.AddSingleton<ICommandRouter<CloudEvent>, GetAllDevicesCommandRouter>();
            builder.Services.AddSingleton<ICommandRouter<CloudEvent>, DeleteDeviceCommandRouter>();

            builder.Services.AddSingleton(mqttSettings);
            builder.Services.AddSingleton<IPubSubClient<CloudEvent>, PubSubClient<CloudEvent>>();
            builder.Services.AddSingleton<CommandDispatcher<CloudEvent>>();

            builder.Services.AddDbContext<DeviceRepository>(opt => opt.UseInMemoryDatabase("DeviceList"));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        }

        private static MqttSettings LoadMqttSettings(ConfigurationManager config)
        {
            var mqttSettings = config.GetSection(Mqtt_Settings_Node).Get<MqttSettings>();
            if (mqttSettings is null)
            {
                _logger?.LogError("Could not find MQTT settings in configuration.");
                Environment.Exit(0);
            }
            return mqttSettings;
        }
    }
}