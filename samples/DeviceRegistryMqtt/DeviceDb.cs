using Microsoft.EntityFrameworkCore;

namespace DeviceRegistry
{
    class DeviceDb : DbContext
    {
        public DeviceDb(DbContextOptions<DeviceDb> options)
            : base(options) { }

        public DbSet<Device> Devices => Set<Device>();
    }
}