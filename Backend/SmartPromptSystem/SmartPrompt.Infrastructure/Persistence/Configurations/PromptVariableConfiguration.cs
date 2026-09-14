using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartPrompt.Domain.Entities;

namespace SmartPrompt.Infrastructure.Persistence.Configurations;

public class PromptVariableConfiguration : IEntityTypeConfiguration<PromptVariable>
{
    public void Configure(EntityTypeBuilder<PromptVariable> builder)
    {
        builder.HasKey(pv => pv.Id);

        builder.Property(pv => pv.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(pv => pv.Prompt)
            .WithMany(p => p.Variables)
            .HasForeignKey(pv => pv.PromptId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(pv => new { pv.PromptId, pv.Name }).IsUnique();
    }
}
