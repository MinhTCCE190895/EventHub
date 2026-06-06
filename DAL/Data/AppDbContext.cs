using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace DAL.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<EventCategory> EventCategories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<EventTag> EventTags { get; set; }
    public DbSet<EventRequest> EventRequests { get; set; }
    public DbSet<Bookmark> Bookmarks { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<EventComment> EventComments { get; set; }
    public DbSet<EventReminder> EventReminders { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<FeedbackDetail> FeedbackDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Cấu hình từ Assembly chứa AppDbContext (chính là project DAL này)
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
