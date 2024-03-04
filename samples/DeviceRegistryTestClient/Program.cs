using Azure.Messaging;
using CommandDispatcher.Mqtt.CloudEvents;
using CommandDispatcher.Mqtt.Core;
using CommandDispatcher.Mqtt.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace DeviceRegistryTestClient
{
    internal static class Program
    {
        private const string Mqtt_Settings_Node = "MqttSettings";
        private static ILogger? _logger;
        private static MqttSettings? _mqttSettings;
        private static string? _commandTopic;
        private static string? _responseTopic;
        private static IPubSubClient<CloudEvent>? _publisher;
        private static readonly Random _random = new();
        private static readonly Dictionary<string, string> _deviceOptions = new()
        {
                { "Thermostat", "Temperature Sensor" },
                { "Camera", "Thermal Camera" },
                { "DoorSensor", "Door Sensor" },
                { "MotionSensor", "Motion Sensor" },
                { "SmokeDetector", "Smoke Detector" },
                { "WaterLeakDetector", "Water Leak Detector" },
                { "SmartPlug", "Smart Plug" },
                { "SmartLight", "Smart Light" },
                { "SmartLock", "Smart Lock" },
                { "SmartThermostat", "Smart Thermostat" },
                { "SmartCamera", "Smart Camera" },
                { "SmartDoorSensor", "Smart Door Sensor" },
                { "SmartMotionSensor", "Smart Motion Sensor" },
                { "SmartSmokeDetector", "Smart Smoke Detector" },
                { "SmartWaterLeakDetector", "Smart Water Leak Detector" },
            };

        static async Task Main(string[] args)
        {
            await Initialize();
            Thread.Sleep(500);
            await ReadInputAndExecute();
        }

        private async static Task Initialize()
        {
            var loggerFactory = InitLoggingFactory();
            _logger = loggerFactory.CreateLogger("Program");

            LoadConfiguration();

            _publisher = new PubSubClient<CloudEvent>(_mqttSettings!, loggerFactory.CreateLogger<PubSubClient<CloudEvent>>());
            var subscriber = new PubSubClient<CloudEvent>(_mqttSettings!, loggerFactory.CreateLogger<PubSubClient<CloudEvent>>());
            await subscriber.Subscribe(_responseTopic!, HandleResponses);
        }

#pragma warning disable S2190 // Loops and recursions should not be infinite
        private static async Task ReadInputAndExecute()
#pragma warning restore S2190 // Loops and recursions should not be infinite
        {
            PrintInstructions();

            var key = Console.ReadKey();

            switch (key.KeyChar)
            {
                case '1':
                    await CreateDevice();
                    break;
                case '2':
                    GetAllDevices();
                    break;
                case '3':
                    DeleteDevice();
                    break;
                case '4':
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid option");
                    break;
            }

            await ReadInputAndExecute();
        }

        private static void PrintInstructions()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("1. Create a device");
            Console.WriteLine("2. Get all devices");
            Console.WriteLine("3. Delete device");
            Console.WriteLine("4. Exit");
            Console.ResetColor();
        }

        private static async Task CreateDevice()
        {
            var deviceInfo = _deviceOptions.ElementAt(_random.Next(_deviceOptions!.Count));
            int value = _random.Next(1000);
            string seed = value.ToString("000");
            var device = new
            {
                Id = Guid.NewGuid(),
                Name = $"{deviceInfo.Key}{seed}",
                Type = deviceInfo.Value,
                IsOnline = _random.Next(2) == 0
            };
            var cloudEvent = new CloudEvent("DeviceRegistryService", "CreateDevice", device);
            cloudEvent.SetCorrelationId(Guid.NewGuid().ToString());

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine($"Creating device: {device}");
            Console.ResetColor();

            await _publisher!.Publish(_commandTopic!, cloudEvent);

        }

        private static void GetAllDevices()
        {
            var cloudEvent = new CloudEvent("DeviceRegistryService", "GetAllDevices", null);
            cloudEvent.SetCorrelationId(Guid.NewGuid().ToString());

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("Getting all devices");
            Console.ResetColor();

            _publisher!.Publish(_commandTopic!, cloudEvent);
        }

        private static void DeleteDevice()
        {
            Console.WriteLine();
            Console.WriteLine("Input device Id:");
            var deviceId = Console.ReadLine();

            if (string.IsNullOrEmpty(deviceId))
            {
                Console.WriteLine();
                Console.WriteLine("Invalid device Id");
                return;
            }

            var cloudEvent = new CloudEvent("DeviceRegistryService", "DeleteDevice", deviceId);
            cloudEvent.SetCorrelationId(Guid.NewGuid().ToString());

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine($"Deleting device with Id: {deviceId}");
            Console.ResetColor();

            _publisher!.Publish(_commandTopic!, cloudEvent);
        }

        private static ILoggerFactory InitLoggingFactory()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddConsole();
                builder.AddEventSourceLogger();
            });
            return loggerFactory;
        }

        private static void LoadConfiguration()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
            _mqttSettings = config.GetSection(Mqtt_Settings_Node).Get<MqttSettings>();

            ArgumentNullException.ThrowIfNull(_mqttSettings);
            ArgumentException.ThrowIfNullOrEmpty(config["DeviceRegistry:IncomingTopic"]);
            ArgumentException.ThrowIfNullOrEmpty(config["DeviceRegistry:OutgoingTopic"]);

            _commandTopic = config["DeviceRegistry:IncomingTopic"]!;
            _responseTopic = config["DeviceRegistry:OutgoingTopic"]!;
        }

        private static Task HandleResponses(string topic, CloudEvent message)
        {
            var response = JsonSerializer.Serialize(message, new JsonSerializerOptions() { WriteIndented = true});

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine($"Response: {response}");
            Console.WriteLine();
            Console.ResetColor();

            // Let response finish printing before printing next instructions
            Thread.Sleep(200);
            PrintInstructions();

            return Task.CompletedTask;
        }

        public static string Prettify(string jsonString)
        {
            using var stream = new MemoryStream();
            var document = JsonDocument.Parse(jsonString);
            var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
            document.WriteTo(writer);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}
