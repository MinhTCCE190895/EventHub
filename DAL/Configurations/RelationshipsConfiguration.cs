using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

public class RelationshipsConfiguration : 
    IEntityTypeConfiguration<EventComment>,
    IEntityTypeConfiguration<EventRequest>,
    IEntityTypeConfiguration<Booking>,
    IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<EventComment> builder)
    {
        builder.HasOne(c => c.User)
            .WithMany(u => u.EventComments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Event)
            .WithMany(e => e.EventComments)
            .HasForeignKey(c => c.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EventRequest> builder)
    {
        builder.HasOne(er => er.Student)
            .WithMany(u => u.EventRequests)
            .HasForeignKey(er => er.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasIndex(b => new { b.StudentId, b.EventId }).IsUnique();

        builder.HasOne(b => b.Student)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Event)
            .WithMany(e => e.Bookings)
            .HasForeignKey(b => b.EventId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.HasOne(f => f.Booking)
            .WithOne(b => b.Feedback)
            .HasForeignKey<Feedback>(f => f.BookingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
