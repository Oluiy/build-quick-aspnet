namespace create_aspnet_app.Templates;

/// <summary>
/// Produces the generated API project's <c>Program.cs</c> for the minimal hosting model.
/// </summary>
internal static class ProgramTemplate
{
    /// <summary>Builds Program.cs content wired up with CORS, Swagger, and a sample health-check endpoint.</summary>
    public static string Generate(string projectName) => $$"""
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Cross-Origin Resource Sharing
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            });
        });

        // 2. Add Swagger / OpenAPI services
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "{{projectName}} API",
                Version = "v1",
                Description = "Clean Architecture Web API scaffolding"
            });
        });

        var app = builder.Build();

        // 3. Configure Middleware Pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "{{projectName}} API v1");
                // Serves Swagger UI directly at the root URL (http://localhost:<port>/)
                c.RoutePrefix = string.Empty;
            });
        }

        if (app.Environment.IsProduction())
        {
            app.UseHttpsRedirection();
        }

        app.UseCors("AllowAll");

        // 4. Working Sample Endpoints
        app.MapGet("/api/health", () => Results.Ok(new
        {
            Status = "Healthy",
            Project = "{{projectName}} API",
            Timestamp = DateTime.UtcNow
        }))
        .WithName("HealthCheck");

        app.Run();
    }   
    // Exposes the implicit Program class so WebApplicationFactory<Program> can bootstrap it in tests.
    public partial class Program { }
    """;
}
