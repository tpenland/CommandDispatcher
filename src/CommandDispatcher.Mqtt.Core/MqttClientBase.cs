using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace CommandDispatcher.Mqtt.Core
{
    /// <summary>
    /// Abstract base class that handles the connection to the broker.
    /// </summary>
    public abstract class MqttClientBase: IDisposable
    {
        public string MqttClientId => _mqttClient!.InternalClient.Options.ClientId;

        protected readonly MqttSettings _mqttSettings;
        protected readonly ILogger _logger;
        protected IManagedMqttClient? _mqttClient;

        protected MqttClientBase(MqttSettings mqttSettings, ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(mqttSettings);
            ArgumentNullException.ThrowIfNull(logger);

            _mqttSettings = mqttSettings;
            _logger = logger;
        }

        /// <summary>
        /// Performs the necessary setup to initialize and start the client.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task StartAsync()
        {
            _logger.LogInformation("Starting MQTT client.");
            _mqttClient = new MqttFactory().CreateManagedMqttClient();
            await InitializeConnection();
        }

        /// <summary>
        /// Starts the connection to the MQTT broker and sets up event handlers for the connection.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task InitializeConnection()
        {
            MqttClientOptions mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(_mqttSettings.ServerAddress, _mqttSettings.ServerPort)
                .Build();

            ManagedMqttClientOptions managedMqttClientOptions = new ManagedMqttClientOptionsBuilder()
                .WithClientOptions(mqttClientOptions)
                .Build();

            _mqttClient!.ConnectingFailedAsync += MqttClient_ConnectingFailedAsync;
            _mqttClient!.ConnectedAsync += MqttClient_ConnectedAsync;

            _logger.LogInformation("Connecting to broker: {address}:{port}", _mqttSettings!.ServerAddress, _mqttSettings!.ServerPort);
            await _mqttClient!.StartAsync(managedMqttClientOptions);
        }

        protected virtual Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            _logger.LogInformation("Successfully connected with result: {result}", arg.ConnectResult.ResultCode);
            return Task.CompletedTask;
        }

        protected virtual Task MqttClient_ConnectingFailedAsync(ConnectingFailedEventArgs arg)
        {
            _logger.LogCritical("Unable to connect {arg}", arg.Exception);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _mqttClient?.Dispose();
            }
        }
    }
}
