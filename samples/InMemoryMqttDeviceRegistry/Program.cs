using DeviceRegistry;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddDbContext<DeviceDb>(opt => opt.UseInMemoryDatabase("DeviceList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.Run();

public static async Task<IEnumerable<Device>> GetAllDevices(DeviceDb db)
{
    return await db.Devices.ToArrayAsync();
}

public static async Task<IEnumerable<Device>> GetOnlineDevices(DeviceDb db) 
{
    return await db.Devices.Where(t => t.IsOnline).ToListAsync();
}

public static async Task<IEnumerable<Device>> GetDevicesByType(string type, DeviceDb db) 
{
    return await db.Devices.Where(t => t.Type == type).ToListAsync();
}

public static async Task<Device?> GetDevice(int id, DeviceDb db)
{
    return await db.Devices.FindAsync(id)
        is Device device
            ? device
            : null;
}

public static async Task<Device> CreateDevice(Device device, DeviceDb db)
{
    db.Devices.Add(device);
    await db.SaveChangesAsync();

    return device;
}

public static async Task UpdateDevice(int id, Device device, DeviceDb db)
{
    var foundDevice = await db.Devices.FindAsync(id);

    if (foundDevice is null) return;

    foundDevice.Name = device.Name;
    foundDevice.Type = device.Type;
    foundDevice.IsOnline = device.IsOnline;

    await db.SaveChangesAsync();
}

public static async Task DeleteDevice(int id, DeviceDb db)
{
    if (await db.Devices.FindAsync(id) is Device device)
    {
        db.Devices.Remove(device);
        await db.SaveChangesAsync();
    }
}
