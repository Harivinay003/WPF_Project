using Microsoft.EntityFrameworkCore;
using VirtualEMS.Library;
using System.Configuration;

namespace VirtualEMS.DataServices
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
             : base(options)
        {
        }
        public DbSet<AlarmTag> AlarmTags { get; set; }
        public DbSet<AlarmParameter> AlarmParameters { get; set; }
        public DbSet<IODevice> IODevices { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<FieldDevice> FieldDevices { get; set; }
        public DbSet<SerialDevice> SerialDevices { get; set; }
        public DbSet<SerialDeviceDriver> SerialDeviceDrivers { get; set; }
        public DbSet<SerialDeviceParameter> SerialDeviceParameters { get; set; }
        public DbSet<SerialDeviceReadBlock> SerialDeviceReadBlocks { get; set; }
        public DbSet<SerialDeviceRegister> SerialDeviceRegisters { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<PageItem> PageItems { get; set; }
        public DbSet<PageItemTag> PageItemTags { get; set; }
        public DbSet<PageItemParameter> PageItemParameters { get; set; }
        public DbSet<SLDPage> SLDPages { get; set; }
        public DbSet<SLDPageItem> SLDPageItems { get; set; }
        public DbSet<SLDPageItemTag> SLDPageItemTags { get; set; }
        public DbSet<SLDPageItemParameter> SLDPageItemParameters { get; set; }
        public DbSet<Node> Nodes { get; set; }
        public DbSet<ItemCalculation> ItemCalculations { get; set; }
        public DbSet<ItemCalculationComponent> ItemCalculationComponents { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupItem> GroupItems { get; set; }
        public DbSet<TODTarrif> TODTarrifs { get; set; }
        public DbSet<TrendTag>  TrendTags { get; set; }
        public DbSet<TrendParameter> TrendParameters { get; set; }
        public DbSet<VirtualEMS.Library.User> Users { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           if(!optionsBuilder.IsConfigured)
           {
                // Configure the context here if not already configured
                optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["ConfigDBConnString"].ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
