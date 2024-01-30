namespace CommandDispatcher.Mqtt.Core
{
    /// <summary>
    /// Settings for the MQTT connection.
    /// </summary>
    public record MqttSettings
    {
        /// <summary>
        /// The address of the MQTT server.
        /// </summary>
        public required string ServerAddress { get; set; }
        /// <summary>
        /// The port of the MQTT server.
        /// </summary>
        public int ServerPort { get; set; } = 1883;
        /// <summary>
        /// The Mqtt QoS to use when sending messages.
        /// </summary>
        public int MqttQualityOfServiceLevel { get; set; } = 1;
    }
}
