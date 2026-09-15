using Microsoft.EntityFrameworkCore;
using SmartPrompt.Domain.Entities;

namespace SmartPrompt.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Category> Categories { get; }
    DbSet<Prompt> Prompts { get; }
    DbSet<PromptVariable> PromptVariables { get; }
    DbSet<PromptTemplate> PromptTemplates { get; }
    DbSet<PromptVersion> PromptVersions { get; }
    DbSet<PromptExecution> PromptExecutions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
