using JewelryHub.Domain.Unions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryHub.Persistence.Configurations.Unions;

public class UnionAnnouncementConfiguration : IEntityTypeConfiguration<UnionAnnouncement>
{
    public void Configure(EntityTypeBuilder<UnionAnnouncement> b)
    {
        b.ToTable("UnionAnnouncements");
        b.Property(x => x.Title).IsRequired().HasMaxLength(250);
        b.Property(x => x.Body).IsRequired().HasMaxLength(4000);

        b.HasOne(x => x.PublishedByMember).WithMany().HasForeignKey(x => x.PublishedByMemberId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.UnionId, x.IsPinned, x.CreatedAtUtc });

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class UnionDocumentConfiguration : IEntityTypeConfiguration<UnionDocument>
{
    public void Configure(EntityTypeBuilder<UnionDocument> b)
    {
        b.ToTable("UnionDocuments");
        b.Property(x => x.Title).IsRequired().HasMaxLength(250);
        b.Property(x => x.FileUrl).IsRequired().HasMaxLength(500);

        b.HasOne(x => x.UploadedByMember).WithMany().HasForeignKey(x => x.UploadedByMemberId).OnDelete(DeleteBehavior.Restrict);

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class UnionEventConfiguration : IEntityTypeConfiguration<UnionEvent>
{
    public void Configure(EntityTypeBuilder<UnionEvent> b)
    {
        b.ToTable("UnionEvents");
        b.Property(x => x.Title).IsRequired().HasMaxLength(250);
        b.HasIndex(x => new { x.UnionId, x.StartsAtUtc });

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class UnionPollConfiguration : IEntityTypeConfiguration<UnionPoll>
{
    public void Configure(EntityTypeBuilder<UnionPoll> b)
    {
        b.ToTable("UnionPolls");
        b.Property(x => x.Question).IsRequired().HasMaxLength(500);

        b.HasOne(x => x.CreatedByMember).WithMany().HasForeignKey(x => x.CreatedByMemberId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Options).WithOne(o => o.Poll).HasForeignKey(o => o.PollId).OnDelete(DeleteBehavior.Cascade);

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class PollOptionConfiguration : IEntityTypeConfiguration<PollOption>
{
    public void Configure(EntityTypeBuilder<PollOption> b)
    {
        b.ToTable("PollOptions");
        b.Property(x => x.Text).IsRequired().HasMaxLength(250);
        b.HasMany(x => x.Votes).WithOne(v => v.PollOption).HasForeignKey(v => v.PollOptionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PollVoteConfiguration : IEntityTypeConfiguration<PollVote>
{
    public void Configure(EntityTypeBuilder<PollVote> b)
    {
        b.ToTable("PollVotes");
        // Enforces one vote per member per option at the DB level (single-select
        // polls additionally check "one option total" in the Application layer).
        b.HasIndex(x => new { x.PollOptionId, x.UnionMemberId }).IsUnique();

        b.HasOne(x => x.UnionMember).WithMany().HasForeignKey(x => x.UnionMemberId).OnDelete(DeleteBehavior.Restrict);
    }
}
