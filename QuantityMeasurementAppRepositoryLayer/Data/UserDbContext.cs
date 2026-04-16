using Microsoft.EntityFrameworkCore;
using QuantityMeasurementAppModelLayer.Entity;

namespace QuantityMeasurementAppRepositoryLayer.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext>options) : base(options){}

        public DbSet<UserEntity> Users{ get; set; }
        public DbSet<QuantityMeasurementEntity> Quantity { get; set; }
    }
}
