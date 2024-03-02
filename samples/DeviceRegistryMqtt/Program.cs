using Azure.Messaging;
using CommandDispatcher.Mqtt.Core;
using CommandDispatcher.Mqtt.Interfaces;
using DeviceRegistryMqtt.CommandRouters;
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

        private static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            // Need logger for this code since this cannot be injected
            InitLoggingForProgram();
            // Register all services need to run the Dispatcher
            RegisterServices(builder);

            var host = builder.Build();

            _ = host.Services.GetRequiredService<CommandDispatcher<CloudEvent>>();

            host.Run();
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
            builder.Services.AddSingleton(mqttSettings);
            builder.Services.AddSingleton<IPubSubClient<CloudEvent>, PubSubClient<CloudEvent>>();
            builder.Services.AddSingleton<CommandDispatcher<CloudEvent>>();

            builder.Services.AddScoped<ICommandRouter<CloudEvent>, CreateDeviceCommandRouter>();
            builder.Services.AddScoped<ICommandRouter<CloudEvent>, GetAllDevicesCommandRouter>();

            builder.Services.AddDbContext<DeviceRegistryRepository>(opt => opt.UseInMemoryDatabase("DeviceList"));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            builder.Services.AddScoped<IDeviceRegistryRepository, DeviceRegistryRepository>();
            builder.Services.AddScoped<IDeviceRegistryService, DeviceRegistryService>();
        }

        private static MqttSettings LoadMqttSettings(IConfiguration config)
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