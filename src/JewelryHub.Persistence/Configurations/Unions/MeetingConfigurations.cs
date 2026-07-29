using JewelryHub.Domain.Unions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryHub.Persistence.Configurations.Unions;

public class MeetingConfiguration : IEntityTypeConfiguration<Meeting>
{
    public void Configure(EntityTypeBuilder<Meeting> b)
    {
        b.ToTable("Meetings");
        b.Property(x => x.Title).IsRequired().HasMaxLength(250);

        b.HasOne(x => x.CreatedByMember).WithMany().HasForeignKey(x => x.CreatedByMemberId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.AgendaItems).WithOne(a => a.Meeting).HasForeignKey(a => a.MeetingId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Attendees).WithOne(a => a.Meeting).HasForeignKey(a => a.MeetingId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Minutes).WithOne(m => m.Meeting).HasForeignKey(m => m.MeetingId).OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.UnionId, x.ScheduledAtUtc });

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class MeetingAgendaItemConfiguration : IEntityTypeConfiguration<MeetingAgendaItem>
{
    public void Configure(EntityTypeBuilder<MeetingAgendaItem> b)
    {
        b.ToTable("MeetingAgendaItems");
        b.Property(x => x.Topic).IsRequired().HasMaxLength(250);
        b.HasIndex(x => new { x.MeetingId, x.Status });
    }
}

public class MeetingAttendeeConfiguration : IEntityTypeConfiguration<MeetingAttendee>
{
    public void Configure(EntityTypeBuilder<MeetingAttendee> b)
    {
        b.ToTable("MeetingAttendees");
        b.HasIndex(x => new { x.MeetingId, x.UnionMemberId }).IsUnique();

        b.HasOne(x => x.UnionMember).WithMany().HasForeignKey(x => x.UnionMemberId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class MeetingMinuteConfiguration : IEntityTypeConfiguration<MeetingMinute>
{
    public void Configure(EntityTypeBuilder<MeetingMinute> b)
    {
        b.ToTable("MeetingMinutes");
        b.Property(x => x.DecisionSummary).IsRequired().HasMaxLength(2000);

        b.HasOne(x => x.AgendaItem).WithMany(a => a.RelatedMinutes).HasForeignKey(x => x.AgendaItemId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.RecordedByMember).WithMany().HasForeignKey(x => x.RecordedByMemberId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.ActionItems).WithOne(a => a.MeetingMinute).HasForeignKey(a => a.MeetingMinuteId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ActionItemConfiguration : IEntityTypeConfiguration<ActionItem>
{
    public void Configure(EntityTypeBuilder<ActionItem> b)
    {
        b.ToTable("ActionItems");
        b.Property(x => x.Description).IsRequired().HasMaxLength(1000);

        b.HasOne(x => x.ResponsibleMember).WithMany().HasForeignKey(x => x.ResponsibleMemberId).OnDelete(DeleteBehavior.Restrict);

        // "My open action items" is the member dashboard's core query.
        b.HasIndex(x => new { x.ResponsibleMemberId, x.Status });
    }
}
