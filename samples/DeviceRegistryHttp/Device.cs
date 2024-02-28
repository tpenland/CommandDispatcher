namespace DeviceRegistry
{
    public class Device
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public bool IsOnline { get; set; }
    }
}