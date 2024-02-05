using CommandDispatcher.Mqtt.Interfaces;
using System.Net.Mime;
using System.Text.Json;

namespace CommandDispatcher.Mqtt.Core
{
    public class JsonEventFormatter<T> : IMqttMessageFormatter<T>
    {
        public T DecodeMessage(byte[] message, ContentType? contentType = null)
        {
            Guard.IsNull(message);

            var decodedMessage = JsonSerializer.Deserialize<T>(message);
            return decodedMessage is null
                ? throw new InvalidOperationException($"Unable to deserialize message of type {typeof(T).Name}.")
                : decodedMessage;
        }

        public byte[] EncodeMessage(T message, out ContentType? contentType)
        {
            contentType = new ContentType("application/json");
            return JsonSerializer.SerializeToUtf8Bytes(message);
        }
    }
}
