using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartPrompt.Domain.Entities;

namespace SmartPrompt.Infrastructure.Persistence.Configurations;

public class PromptExecutionLogConfiguration : IEntityTypeConfiguration<PromptExecutionLog>
{
    public void Configure(EntityTypeBuilder<PromptExecutionLog> builder)
    {
        builder.ToTable("PromptExecutionLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ExecutedAt)
            .IsRequired();

        builder.HasOne(e => e.Prompt)
            .WithMany(p => p.ExecutionLogs)
            .HasForeignKey(e => e.PromptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
