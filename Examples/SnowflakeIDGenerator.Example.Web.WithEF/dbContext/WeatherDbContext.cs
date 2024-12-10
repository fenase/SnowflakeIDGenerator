using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SnowflakeID;
using SnowflakeIDGenerator.Example.Web.WithEF.dbGenerators;
using SnowflakeIDGenerator.Example.Web.WithEF.Model;

namespace SnowflakeIDGenerator.Example.Web.WithEF.dbContext
{
    public class WeatherDbContext : DbContext
    {
        public WeatherDbContext() : base() { }

        public WeatherDbContext(DbContextOptions options) : base(options) { }

        public DbSet<WeatherForecast> WeatherForecasts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var snowflakeConverter = new ValueConverter<Snowflake, ulong>(
                v => v.ToUInt64(),
                v => Snowflake.Parse(v)
            );

            modelBuilder.Entity<WeatherForecast>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<WeatherForecast>()
                .Property(x => x.Id)
                .HasConversion(snowflakeConverter)
                .HasValueGenerator<SnowflakeValueGenerator>();
        }
    }
}
