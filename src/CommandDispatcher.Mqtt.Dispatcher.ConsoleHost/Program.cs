using CloudNative.CloudEvents;
using CommandDispatcher.Mqtt.Core;
using CommandDispatcher.Mqtt.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace CommandDispatcher.Mqtt.Dispatcher.ConsoleHost
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
            // Register all CommandRouters from internally defined class and external DLLs
            RegisterCommandRouters(builder);

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
            builder.Services.AddSingleton(mqttSettings);
            builder.Services.AddSingleton<IPubSubClient<CloudEvent>, PubSubClient<CloudEvent>>();
            builder.Services.AddSingleton<CommandDispatcher<CloudEvent>>();
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

        private static void RegisterCommandRouters(HostApplicationBuilder builder)
        {
            var CommandRouterDlls = builder.Configuration.GetSection("CommandRouterDlls").Get<string[]>();

            var CommandRouters = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => !x.IsAbstract && x.IsClass && typeof(ICommandRouter<CloudEvent>).IsAssignableFrom(x));

            foreach (var router in CommandRouters)
            {
                builder.Services.Add(new ServiceDescriptor(typeof(ICommandRouter<CloudEvent>), router, ServiceLifetime.Scoped));
            }

            if (CommandRouterDlls is null)
            {
                return;
            }

            foreach (var dll in CommandRouterDlls)
            {
                if (!File.Exists(dll))
                {
                    _logger!.LogInformation("Could not find message router dll {dll}", dll);
                    continue;
                }
                Assembly assembly = LoadPlugin(dll);

                var types = assembly.GetTypes();

                if (types == null || !types.Any())
                {
                    _logger!.LogWarning("No types found in {dll}", dll);
                    continue;
                }

                foreach (var (type, result) in from Type type in types
                                               where typeof(IRegisterCommandRouters).IsAssignableFrom(type)
                                               let result = Activator.CreateInstance(type) as IRegisterCommandRouters
                                               select (type, result))
                {
                    if (result is null)
                    {
                        _logger!.LogWarning("Could not create instance of {type}", type);
                        continue;
                    }

                    result.RegisterCommandRouters(builder.Services);
                }
            }

            var routersLoaded = builder.Services.Count(x => x.ServiceType == typeof(ICommandRouter<CloudEvent>));
            if (routersLoaded == 0)
            {
                _logger!.LogError("No message routers were loaded. Process will shutdown.");
                Environment.Exit(0);
            }
            else
            {
                _logger!.LogInformation("Loaded {routersLoaded} message routers.", routersLoaded);
            }
        }

        private static Assembly LoadPlugin(string dllPath)
        {
            PluginLoadContext loadContext = new PluginLoadContext(dllPath);
            return loadContext.LoadFromAssemblyName(AssemblyName.GetAssemblyName(dllPath));
        }
    }
}
