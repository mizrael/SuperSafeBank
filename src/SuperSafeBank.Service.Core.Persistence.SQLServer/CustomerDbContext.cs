using Microsoft.EntityFrameworkCore;

namespace SuperSafeBank.Service.Core.Persistence.SQLServer
{
    public class CustomerDbContext : DbContext
    {
        public CustomerDbContext(DbContextOptions<CustomerDbContext> opts) : base(opts) => Database.EnsureCreated();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomerEmail>(builder =>
            {
                builder.HasKey(e => new { e.CustomerId, e.Email});
            });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<CustomerEmail> CustomerEmails { get; set; }
    }
}