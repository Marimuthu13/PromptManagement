using SmartPrompt.API.Middleware;
using SmartPrompt.Application;
using SmartPrompt.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Centralized exception handling and Problem Details
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Register Application layer dependencies
builder.Services.AddApplication();

// Register Infrastructure layer dependencies
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler(); // Adds the exception handling middleware

app.UseHttpsRedirection();

// Basic health check endpoint
app.MapGet("/", () => "Smart Prompt System API is running.")
   .WithName("GetRoot");

// Detailed health status endpoint
app.MapGet("/api/health", () => Results.Ok(new 
{
    status = "Healthy",
    application = "SmartPromptSystem",
    timestampUtc = DateTime.UtcNow
}))
.WithName("GetHealthStatus");

app.Run();
