namespace DeviceRegistryMqtt
{
    public interface IDeviceRegistryService
    {
        Task<Device> CreateDevice(Device device);
        Task DeleteDevice(int id);
        Task<IEnumerable<Device>> GetAllDevices();
        Task<Device?> GetDevice(int id);
        Task<IEnumerable<Device>> GetDevicesByType(string type);
        Task<IEnumerable<Device>> GetOnLineDevices();
        Task UpdateDevice(int id, Device device);
    }
}