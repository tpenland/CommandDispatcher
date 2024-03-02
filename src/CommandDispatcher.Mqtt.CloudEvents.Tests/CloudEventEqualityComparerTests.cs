using Azure.Messaging;

namespace CommandDispatcher.Mqtt.CloudEvents.Tests
{
    public class CloudEventEqualityComparerTests
    {
        [Fact]
        public void Equals_NullCloudEvents_ReturnsTrue()
        {
            var comparer = new CloudEventEqualityComparer();
            Assert.True(comparer.Equals(null, null));
        }

        [Fact]
        public void Equals_OneNullCloudEvent_ReturnsFalse()
        {
            var comparer = new CloudEventEqualityComparer();
            var cloudEvent = new CloudEvent("CloudEventEqualityComparerTests", "testEvent", "test");
            Assert.False(comparer.Equals(cloudEvent, null));
            Assert.False(comparer.Equals(null, cloudEvent));
        }

        [Fact]
        public void Equals_DifferentCloudEvents_ReturnsFalse()
        {
            var comparer = new CloudEventEqualityComparer();
            var cloudEvent1 = new CloudEvent("CloudEventGenerator", "TestMessage", "test");
            var cloudEvent2 = new CloudEvent("CloudEventGenerator", "TestMessage", "test");

            Assert.False(comparer.Equals(cloudEvent1, cloudEvent2));
        }

        [Fact]
        public void Equals_SameCloudEvents_ReturnsTrue()
        {
            var comparer = new CloudEventEqualityComparer();
            var cloudEvent = new CloudEvent("CloudEventGenerator", "TestMessage", "test");

            Assert.True(comparer.Equals(cloudEvent, cloudEvent));
        }

        [Fact]
        public void GetHashCode_ReturnsHashCode()
        {
            var comparer = new CloudEventEqualityComparer();
            var cloudEvent = new CloudEvent("CloudEventGenerator", "TestMessage", "test");

            Assert.Equal(cloudEvent.GetHashCode(), comparer.GetHashCode(cloudEvent));
        }
    }
}
