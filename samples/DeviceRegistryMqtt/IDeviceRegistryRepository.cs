namespace DeviceRegistryMqtt
{
    public interface IDeviceRegistryRepository
    {
        Task<IEnumerable<Device>> GetAllDevices();
        Task<IEnumerable<Device>> GetOnLineDevices();
        Task<IEnumerable<Device>> GetDevicesByType(string type);
        Task<Device?> GetDevice(int id);
        Task<Device> CreateDevice(Device device);
        Task UpdateDevice(int id, Device device);
        Task DeleteDevice(int id);
    }
}
