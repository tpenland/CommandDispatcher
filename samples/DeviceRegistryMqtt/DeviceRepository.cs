﻿using DeviceRegistryMqtt.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeviceRegistryMqtt
{
    public class DeviceRepository: DbContext, IDeviceRepository
	{
        internal DbSet<Device> Devices => Set<Device>();

        public DeviceRepository(DbContextOptions<DeviceRepository> options) : base(options) { }

        public async Task<IEnumerable<Device>> GetAllDevices()
        {
            return await Devices.ToArrayAsync();
        }

        public async Task<IEnumerable<Device>> GetOnlineDevices()
        {
            return await Devices.Where(t => t.IsOnline).ToListAsync();
        }

        public async Task<IEnumerable<Device>> GetDevicesByType(string type)
        {
            return await Devices.Where(t => t.Type == type).ToListAsync();
        }

        public async Task<Device?> GetDevice(string id)
        {
            return await Devices.FindAsync(id)
                is Device device
                    ? device
                    : null;
        }

        public async Task<Device> CreateDevice(Device device)
        {
            Devices.Add(device);
            await SaveChangesAsync();

            return device;
        }

        public async Task UpdateDevice(string id, Device device)
        {
            var foundDevice = await Devices.FindAsync(id);

            if (foundDevice is null) return;

            foundDevice.Name = device.Name;
            foundDevice.Type = device.Type;
            foundDevice.IsOnline = device.IsOnline;

            await SaveChangesAsync();
        }

        public async Task DeleteDevice(string id)
        {
            if (await Devices.FindAsync(id) is Device device)
            {
                Devices.Remove(device);
                await SaveChangesAsync();
            }
        }
    }
}