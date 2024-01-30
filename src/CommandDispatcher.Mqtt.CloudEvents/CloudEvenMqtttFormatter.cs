using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Core;
using CloudNative.CloudEvents.SystemTextJson;
using CommandDispatcher.Mqtt.Interfaces;
using System.Net.Mime;

namespace CommandDispatcher.Mqtt.CloudEvents
{
    public class CloudEvenMqttFormatter : IMqttMessageFormatter<CloudEvent>
    {
        public CloudEvent DecodeMessage(byte[] message, ContentType? contentType = null)
        {
            var extensionAttributes = CorrelationId.AllAttributes;
            return new JsonEventFormatter().DecodeStructuredModeMessage(message, contentType: contentType, extensionAttributes);
        }

        public byte[] EncodeMessage(CloudEvent message, out ContentType? contentType)
        {
            var formattedMessage = new JsonEventFormatter().EncodeStructuredModeMessage(message, out var formattedContentType);
            contentType = formattedContentType;
            return BinaryDataUtilities.AsArray(formattedMessage);
        }
    }
}
