using SmartPrompt.Application.Common.Models.AI;

namespace SmartPrompt.Application.Common.Interfaces;

public interface IAIProvider
{
    string ProviderName { get; }
    Task<AIResponse> ExecutePromptAsync(AIRequest request, CancellationToken cancellationToken);
}
