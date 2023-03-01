using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace MachineJob
{
    public class DatabaseWriteContext : DbContext
    {
        private static readonly IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
            .Build();

        private static readonly string connectionString = configuration.GetConnectionString("DatabaseWriteConnectionString")!;

        public DbSet<DatabaseWriteEntity> DatabaseWrites { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
    }

    public class DatabaseWriteEntity
    {
        [Key]
        public int Id { get; set; }
        public double? Value1 { get; set; }
        public double? Value2 { get; set; }
        public double? Value3 { get; set; }
        public double? Value4 { get; set; }
        public double? Value5 { get; set; }
        public double? Value6 { get; set; }
        public double? Value7 { get; set; }
        public double? Value8 { get; set; }
        public double? Value9 { get; set; }
        public double? Value10 { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
