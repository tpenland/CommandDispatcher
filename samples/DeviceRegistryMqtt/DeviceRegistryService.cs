namespace DeviceRegistryMqtt
{
    public class DeviceRegistryService : IDeviceRegistryService
    {
        private readonly IDeviceRegistryRepository _repository;

        public DeviceRegistryService(IDeviceRegistryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Device>> GetAllDevices()
        {
            return await _repository.GetAllDevices();
        }

        public async Task<IEnumerable<Device>> GetOnLineDevices()
        {
            return await _repository.GetOnLineDevices();
        }

        public async Task<IEnumerable<Device>> GetDevicesByType(string type)
        {
            return await _repository.GetDevicesByType(type);
        }

        public async Task<Device?> GetDevice(int id)
        {
            return await _repository.GetDevice(id)
                is Device device
                    ? device
                    : null;
        }

        public async Task<Device> CreateDevice(Device device)
        {
            return await _repository.CreateDevice(device);
        }

        public async Task UpdateDevice(int id, Device device)
        {
            await _repository.UpdateDevice(id, device);
        }

        public async Task DeleteDevice(int id)
        {
            await _repository.DeleteDevice(id);
        }
    }
}
