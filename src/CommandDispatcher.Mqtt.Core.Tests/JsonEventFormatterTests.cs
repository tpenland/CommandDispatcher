using CommandDispatcher.TestHelpers;

namespace CommandDispatcher.Mqtt.Core.Tests
{
    public class JsonEventFormatterTests
    {
        [Fact]
        public void Encode_Decode_Successful()
        {
            var samplePoints = new List<SamplePoint>();
            for (int i = 0; i < 10; i++)
            {
                samplePoints.Add(SamplePoint.GetSamplePoint());
            }

            var sut = new JsonEventFormatter<SamplePoint>();
            foreach (var samplePoint in samplePoints)
            {
                var encodedMessage = sut.EncodeMessage(samplePoint, out var contentType);
                var decodedMessage = sut.DecodeMessage(encodedMessage, contentType);
                Assert.True(contentType?.MediaType == "application/json");
                Assert.Equal(samplePoint, decodedMessage);
            }
        }
    }
}
