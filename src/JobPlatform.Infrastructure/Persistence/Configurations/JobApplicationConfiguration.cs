using JobPlatform.Domain.Entities;
using JobPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPlatform.Infrastructure.Persistence.Configurations;

public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.ApplicantId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne<Job>()
            .WithMany()
            .HasForeignKey(a => a.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique filtered index on (JobId, ApplicantId) WHERE [Status] = 0 AND [IsDeleted] = 0
        builder.HasIndex(t => new { t.JobId, t.ApplicantId })
            .IsUnique()
            .HasFilter("[Status] = 0 AND [IsDeleted] = 0");
    }
}
