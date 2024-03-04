namespace DeviceRegistryMqtt.Interfaces
{
    public interface IDeviceRegistryService
    {
        Task<IEnumerable<Device>> GetAllDevices();

        Task<IEnumerable<Device>> GetOnlineDevices();

        Task<IEnumerable<Device>> GetDevicesByType(string type);

        Task<Device?> GetDevice(string id);

        Task<Device> CreateDevice(Device device);

        Task UpdateDevice(string id, Device device);

        Task DeleteDevice(string id);
    }
}
