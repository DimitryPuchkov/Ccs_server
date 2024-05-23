using Microsoft.EntityFrameworkCore;

namespace Ccs_server.data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> users { get; set; } = null!;
        public DbSet<Camera> cameras { get; set; } = null!;
        public DbSet<Value> values { get; set; } = null!;

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=ccs;Username=mainuser;Password=passwd");
        }
    }
}
