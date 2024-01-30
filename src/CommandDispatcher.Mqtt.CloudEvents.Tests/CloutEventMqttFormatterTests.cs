using CloudNative.CloudEvents;
using CommandDispatcher.TestHelpers;
using System.Text.Json;

namespace CommandDispatcher.Mqtt.CloudEvents.Tests
{
    public class CloutEventMqttFormatterTests
    {
        [Fact]
        public void CloudEvenMqttFormatter_EncodeMessage_DecodesSuccessfully()
        {
            var message = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Source = new Uri("urn:CommandDispatcherTests"),
                Type = "TestCommand01",
                Subject = "Test",
                Data = JsonSerializer.Serialize(SamplePoint.GetSamplePoint()),
                Time = DateTime.UtcNow,
            };
            message.SetCorrelationId(Guid.NewGuid().ToString());

            var sut = new CloudEvenMqttFormatter();
            var encodedMessage = sut.EncodeMessage(message, out var contentType);
            var decodedMessage = sut.DecodeMessage(encodedMessage, contentType);
            Assert.True(contentType?.MediaType == "application/cloudevents+json");
            Assert.Equal(message, decodedMessage, new CloudEventEqualityComparer());
        }
    }
}