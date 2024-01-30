namespace CommandDispatcher.TestHelpers
{
    public record SamplePoint
    {
        public static SamplePoint GetSamplePoint()
        {
            var random = new Random();
            var deviceId = $"test{random.Next(1, 100)}";
            (string, string)[] data = [("temp", random.Next().ToString()), ("isOnline", random.NextBool().ToString()), ("errorRate", random.NextDouble().ToString())];
            var pointData = data[random.Next(0, 2)];
            return new SamplePoint(deviceId, pointData.Item1, pointData.Item2, DateTime.UtcNow);
        }

        public SamplePoint()
        {
            
        }

        public SamplePoint(string deviceId, string settingName, string data, DateTime createdAt)
        {
            DeviceId = deviceId;
            SettingName = settingName;
            Data = data;
            CreatedAt = createdAt;
        }
        public string? DeviceId { get; set; }
        public string? SettingName { get; set; }
        public string? Data { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public static class RandomExtensions
    {         
        public static bool NextBool(this Random random)
        {
            var value = random.Next(0, 1);
            return value == 1;
        }
    }
}
