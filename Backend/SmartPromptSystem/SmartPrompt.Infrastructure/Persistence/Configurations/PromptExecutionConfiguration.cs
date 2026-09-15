using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartPrompt.Domain.Entities;

namespace SmartPrompt.Infrastructure.Persistence.Configurations;

public class PromptExecutionConfiguration : IEntityTypeConfiguration<PromptExecution>
{
    public void Configure(EntityTypeBuilder<PromptExecution> builder)
    {
        builder.ToTable("prompt_executions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Provider)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Model)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.ResultContent)
            .IsRequired();
            
        builder.Property(e => e.IsSuccessful)
            .IsRequired();

        builder.HasOne(e => e.Prompt)
            .WithMany(p => p.Executions)
            .HasForeignKey(e => e.PromptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
