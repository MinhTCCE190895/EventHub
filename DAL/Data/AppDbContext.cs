using Microsoft.EntityFrameworkCore;

namespace DAL.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Khai báo DbSet cho các entity ở đây
    // public DbSet<Role> Roles { get; set; }
    // public DbSet<Account> Accounts { get; set; }
    // ...

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Cấu hình Fluent API nếu cần thiết
    }
}
