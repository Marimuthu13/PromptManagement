namespace SmartPrompt.Application.Features.Templates.Queries.GetTemplates;

public class TemplateDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsSystemCurated { get; set; }
}
