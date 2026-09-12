using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SmartPrompt.Domain.Entities;

namespace SmartPrompt.Infrastructure.Persistence;

public class SmartPromptDbContext : DbContext
{
    public SmartPromptDbContext(DbContextOptions<SmartPromptDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Prompt> Prompts => Set<Prompt>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Apply configurations from the assembly (e.g., UserConfiguration, PromptConfiguration)
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
