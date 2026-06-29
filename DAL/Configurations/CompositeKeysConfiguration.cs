using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

public class CompositeKeysConfiguration : 
    IEntityTypeConfiguration<EventCategory>,
    IEntityTypeConfiguration<EventTag>,
    IEntityTypeConfiguration<Bookmark>,
    IEntityTypeConfiguration<Follow>,
    IEntityTypeConfiguration<EventReminder>
{
    public void Configure(EntityTypeBuilder<EventCategory> builder)
    {
        builder.HasKey(ec => new { ec.EventId, ec.CategoryId });

        builder.HasOne(ec => ec.Category)
            .WithMany(c => c.EventCategories)
            .HasForeignKey(ec => ec.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ec => ec.Event)
            .WithMany(e => e.EventCategories)
            .HasForeignKey(ec => ec.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<EventTag> builder)
    {
        builder.HasKey(et => new { et.EventId, et.TagId });

        builder.HasOne(et => et.Tag)
            .WithMany(t => t.EventTags)
            .HasForeignKey(et => et.TagId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(et => et.Event)
            .WithMany(e => e.EventTags)
            .HasForeignKey(et => et.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<Bookmark> builder)
    {
        builder.HasKey(b => new { b.StudentId, b.EventId });
        
        builder.HasOne(b => b.Student)
            .WithMany(u => u.Bookmarks)
            .HasForeignKey(b => b.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Event)
            .WithMany(e => e.Bookmarks)
            .HasForeignKey(b => b.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<Follow> builder)
    {
        builder.HasKey(f => new { f.FollowerId, f.FolloweeId });

        builder.HasOne(f => f.Follower)
            .WithMany(u => u.Followees)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Followee)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FolloweeId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EventReminder> builder)
    {
        builder.HasKey(er => er.EventId);
    }
}
