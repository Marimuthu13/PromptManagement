using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SmartPrompt.API.Middleware;
using SmartPrompt.Application;
using SmartPrompt.Infrastructure;
using SmartPrompt.Infrastructure.Authentication;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure JWT Authentication
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new Microsoft.OpenApi.OpenApiComponents();
        var scheme = new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header using the Bearer scheme."
        };
        document.Components.SecuritySchemes ??= new Dictionary<string, Microsoft.OpenApi.IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes.Add("Bearer", scheme);

        var req = new Microsoft.OpenApi.OpenApiSecurityRequirement();
        req.Add(new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer"), new List<string>());
        
        document.Security ??= new List<Microsoft.OpenApi.OpenApiSecurityRequirement>();
        document.Security.Add(req);

        return Task.CompletedTask;
    });
});

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
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/openapi/v1.json", "SmartPrompt API");
    });
}

app.UseExceptionHandler(); // Adds the exception handling middleware

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

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

public partial class Program { }
