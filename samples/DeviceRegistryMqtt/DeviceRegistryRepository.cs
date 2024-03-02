using Microsoft.EntityFrameworkCore;

namespace DeviceRegistryMqtt
{
    public class DeviceRegistryRepository : DbContext, IDeviceRegistryRepository
    {
        internal DbSet<Device> Devices => Set<Device>();

        public DeviceRegistryRepository(DbContextOptions<DeviceRegistryRepository> options)
            : base(options) { }

        public async Task<Device> CreateDevice(Device device)
        {
            Devices.Add(device);
            await SaveChangesAsync();

            return device;
        }

        public async Task UpdateDevice(int id, Device device)
        {
            var foundDevice = await Devices.FindAsync(id);

            if (foundDevice is null) return;

            foundDevice.Name = device.Name;
            foundDevice.Type = device.Type;
            foundDevice.IsOnline = device.IsOnline;

            await SaveChangesAsync();
        }

        public async Task DeleteDevice(int id)
        {
            if (await Devices.FindAsync(id) is Device device)
            {
                Devices.Remove(device);
                await SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Device>> GetAllDevices()
        {
            return await Devices.ToArrayAsync();
        }

        public async Task<Device?> GetDevice(int id)
        {
            return await Devices.FindAsync(id);
        }

        public async Task<IEnumerable<Device>> GetDevicesByType(string type)
        {
            return await Devices.Where(t => t.Type == type).ToListAsync();
        }

        public async Task<IEnumerable<Device>> GetOnLineDevices()
        {
            return await Devices.Where(t => t.IsOnline).ToListAsync();
        }
    }
}
