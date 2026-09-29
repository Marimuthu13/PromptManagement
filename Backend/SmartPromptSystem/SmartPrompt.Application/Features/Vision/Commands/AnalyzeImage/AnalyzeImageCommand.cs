using MediatR;

namespace SmartPrompt.Application.Features.Vision.Commands.AnalyzeImage;

public record AnalyzeImageCommand : IRequest<string>
{
    public string ImageData { get; init; } = string.Empty;
}
