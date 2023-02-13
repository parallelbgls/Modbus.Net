using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;


namespace MachineJob
{
    public class DatabaseWriteContext : DbContext
    {
        static readonly string connectionString = new ConfigurationBuilder().AddJsonFile($"appsettings.json").Build().GetConnectionString("DatabaseWriteConnectionString")!;

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
