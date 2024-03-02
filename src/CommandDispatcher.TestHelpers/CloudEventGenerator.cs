using Azure.Messaging;
using CommandDispatcher.Mqtt.CloudEvents;
using System.Collections;

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

            var message = new CloudEvent("CloudEventGenerator", "TestMessage", payload);
            message.SetCorrelationId(Guid.NewGuid().ToString());

            return message;
        }
    }

}
