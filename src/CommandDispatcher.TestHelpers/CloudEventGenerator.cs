using CloudNative.CloudEvents;
using System.Collections;
using CommandDispatcher.Mqtt.CloudEvents;

namespace CommandDispatcher.TestHelpers
{
    public class CloudEventGenerator : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                    Generate(1)
            };

            yield return new object[]
            {
                    Generate(3)
            };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public CloudEvent Generate(int samplePointCount)
        {
            string payload;

            if (samplePointCount == 1)
            {
                payload = System.Text.Json.JsonSerializer.Serialize(SamplePoint.GetSamplePoint());
            }
            else
            {
                var samplePoints = new List<SamplePoint>();
                for (int i = 0; i < samplePointCount; i++)
                {
                    samplePoints.Add(SamplePoint.GetSamplePoint());
                }
                payload = System.Text.Json.JsonSerializer.Serialize(samplePoints);
            }

            var message = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Source = new Uri("urn:CloudEventGenerator"),
                Type = "TestMessage",
                Subject = "Test",
                Data = payload,
                Time = DateTime.UtcNow
            };
            message.SetCorrelationId(Guid.NewGuid().ToString());

            return message;
        }
    }

}
