using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MachineJob
{
    public class DatabaseWriteContext : DbContext
    {
        private static readonly IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.default.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
            .Build();

        private static readonly string connectionString = configuration.GetConnectionString("DatabaseWriteConnectionString")!;

        public DbSet<DatabaseWriteEntity>? DatabaseWrites { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
    }

    [Table(name: "databasewrites")]
    public partial class DatabaseWriteEntity
    {
        [Key]
        public int Id { get; set; }

        public DateTime UpdateTime { get; set; }
    }
}
