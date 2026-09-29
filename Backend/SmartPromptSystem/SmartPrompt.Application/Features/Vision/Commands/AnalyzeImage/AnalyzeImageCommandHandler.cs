using System.Text.RegularExpressions;
using MediatR;
using SmartPrompt.Application.Common.Interfaces;
using SmartPrompt.Application.Common.Models.AI;
using SmartPrompt.Domain.Entities;

namespace SmartPrompt.Application.Features.Vision.Commands.AnalyzeImage;

public class AnalyzeImageCommandHandler : IRequestHandler<AnalyzeImageCommand, string>
{
    private readonly IAIProvider _aiProvider;
    private readonly IApplicationDbContext _context;

    public AnalyzeImageCommandHandler(IAIProvider aiProvider, IApplicationDbContext context)
    {
        _aiProvider = aiProvider;
        _context = context;
    }

    public async Task<string> Handle(AnalyzeImageCommand request, CancellationToken cancellationToken)
    {
        // Size Check: Approximate size in bytes from Base64 length
        // Base64 string length is roughly 4/3 of the actual byte size.
        // E.g., 5MB = 5 * 1024 * 1024 = 5242880 bytes. So Base64 length ~ 6,990,506
        if (request.ImageData.Length > 7_000_000)
        {
            throw new Exception("Image size too large. Please upload an image smaller than 5MB.");
        }

        var mimeType = "image/jpeg";
        var base64Data = request.ImageData;

        var commaIndex = request.ImageData.IndexOf(',');
        if (commaIndex >= 0)
        {
            // Extract MIME type
            var match = Regex.Match(request.ImageData.Substring(0, commaIndex), @"data:(image/[a-zA-Z0-9]+);");
            if (match.Success)
            {
                mimeType = match.Groups[1].Value;
            }
            base64Data = request.ImageData.Substring(commaIndex + 1);
        }

        var aiRequest = new AIRequest
        {
            Model = "gemini-3.6-flash",
            SystemPrompt = "You are an expert reverse-engineering AI. Please analyze the attached image and write a detailed, 150-word text prompt that captures its style, lighting, composition, color palette, and specific subjects so that an AI image generator could recreate it accurately.",
            UserPrompt = "Analyze this image and provide the prompt.",
            ImageData = base64Data,
            ImageMimeType = mimeType,
            Temperature = 0.5f
        };

        var response = await _aiProvider.ExecutePromptAsync(aiRequest, cancellationToken);

        // Log token usage (Standalone, no PromptId)
        var log = new PromptExecutionLog
        {
            PromptId = null,
            ExecutedAt = DateTime.UtcNow,
            PromptTokens = response.PromptTokens,
            CompletionTokens = response.CompletionTokens,
            TotalTokens = response.TotalTokens,
            FeatureType = "Vision Analysis",
            DisplayTitle = "Vision: Reverse Engineering",
            Status = "Success",
            IsFallback = false,
            ModelUsed = "gemini-3.6-flash",
            LatencyMs = (long)response.Duration.TotalMilliseconds
        };

        _context.PromptExecutionLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);

        return response.Content;
    }
}
