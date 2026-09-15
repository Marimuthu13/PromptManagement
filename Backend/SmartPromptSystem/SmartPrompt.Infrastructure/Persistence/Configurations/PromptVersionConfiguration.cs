using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartPrompt.Domain.Entities;

namespace SmartPrompt.Infrastructure.Persistence.Configurations;

public class PromptVersionConfiguration : IEntityTypeConfiguration<PromptVersion>
{
    public void Configure(EntityTypeBuilder<PromptVersion> builder)
    {
        builder.ToTable("prompt_versions");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Content)
            .IsRequired();

        builder.Property(v => v.VersionNumber)
            .IsRequired();

        builder.HasOne(v => v.Prompt)
            .WithMany(p => p.Versions)
            .HasForeignKey(v => v.PromptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
