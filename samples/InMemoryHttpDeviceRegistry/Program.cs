using DeviceRegistry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DeviceDb>(opt => opt.UseInMemoryDatabase("DeviceList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

RouteGroupBuilder devices = app.MapGroup("/devices");

devices.MapGet("/", GetAllDevices);
devices.MapGet("/online", GetOnlineDevices);
devices.MapGet("/type", GetDevicesByType);
devices.MapGet("/{id}", GetDevice);
devices.MapPost("/", CreateDevice);
devices.MapPut("/{id}", UpdateDevice);
devices.MapDelete("/{id}", DeleteDevice);

app.Run();

static async Task<IResult> GetAllDevices(DeviceDb db)
{
    return TypedResults.Ok(await db.Devices.ToArrayAsync());
}

static async Task<IResult> GetOnlineDevices(DeviceDb db) 
{
    return TypedResults.Ok(await db.Devices.Where(t => t.IsOnline).ToListAsync());
}

static async Task<IResult> GetDevicesByType([FromQuery] string type, DeviceDb db) 
{
    return TypedResults.Ok(await db.Devices.Where(t => t.Type == type).ToListAsync());
}

static async Task<IResult> GetDevice(int id, DeviceDb db)
{
    return await db.Devices.FindAsync(id)
        is Device device
            ? TypedResults.Ok(device)
            : TypedResults.NotFound();
}

static async Task<IResult> CreateDevice(Device device, DeviceDb db)
{
    db.Devices.Add(device);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/devices/{device.Id}", device);
}

static async Task<IResult> UpdateDevice(int id, Device device, DeviceDb db)
{
    var foundDevice = await db.Devices.FindAsync(id);

    if (foundDevice is null) return TypedResults.NotFound();

    foundDevice.Name = device.Name;
    foundDevice.Type = device.Type;
    foundDevice.IsOnline = device.IsOnline;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> DeleteDevice(int id, DeviceDb db)
{
    if (await db.Devices.FindAsync(id) is Device device)
    {
        db.Devices.Remove(device);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    return TypedResults.NotFound();
}
