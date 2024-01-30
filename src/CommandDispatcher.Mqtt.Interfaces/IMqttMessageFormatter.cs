using System.Net.Mime;

namespace CommandDispatcher.Mqtt.Interfaces
{
    public interface IMqttMessageFormatter<T>
    {
        T DecodeMessage(byte[] message, ContentType? contentType = null);
        byte[] EncodeMessage(T message, out ContentType? contentType);
    }
}
