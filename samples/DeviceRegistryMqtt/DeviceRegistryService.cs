using DeviceRegistryMqtt.Interfaces;
using Microsoft.Extensions.Logging;

namespace DeviceRegistryMqtt
{
    internal class DeviceRegistryService : IDeviceRegistryService
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly ILogger _logger;

        public DeviceRegistryService(IDeviceRepository deviceRepository, ILogger<DeviceRegistryService> logger)
        {
            ArgumentNullException.ThrowIfNull(deviceRepository);
            ArgumentNullException.ThrowIfNull(logger);

            (_deviceRepository, _logger) = (deviceRepository, logger);
            _logger.LogInformation("DeviceRegistryService initialized");
        }

        public async Task<Device> CreateDevice(Device device)
        {
            return await _deviceRepository.CreateDevice(device);
        }

        public async Task DeleteDevice(int id)
        {
            await _deviceRepository.DeleteDevice(id);
        }

        public async Task<IEnumerable<Device>> GetAllDevices()
        {
            return await _deviceRepository.GetAllDevices();
        }

        public async Task<Device?> GetDevice(int id)
        {
            return await _deviceRepository.GetDevice(id);
        }

        public Task<IEnumerable<Device>> GetDevicesByType(string type)
        {
            return _deviceRepository.GetDevicesByType(type);
        }

        public async Task<IEnumerable<Device>> GetOnlineDevices()
        {
            return await _deviceRepository.GetOnlineDevices();
        }

        public async Task UpdateDevice(int id, Device device)
        {
            await _deviceRepository.UpdateDevice(id, device);
        }
    }
}
