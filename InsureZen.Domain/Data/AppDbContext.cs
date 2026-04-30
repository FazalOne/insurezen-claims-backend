using Microsoft.EntityFrameworkCore;
using InsureZen.Domain.Models;

namespace InsureZen.Domain.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Claim> Claims { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Claim>()
            .Property(c => c.Version)
            .IsRowVersion();
    }
}
