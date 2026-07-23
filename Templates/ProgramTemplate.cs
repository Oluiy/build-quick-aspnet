using create_aspnet_app.Scaffolding;

namespace create_aspnet_app.Templates;

/// <summary>
/// Produces the generated API project's <c>Program.cs</c> for the minimal hosting model.
/// </summary>
internal static class ProgramTemplate
{
    /// <summary>
    /// Builds Program.cs content wired up with Serilog, CORS, Swagger, and a sample health-check
    /// endpoint, plus optional Entity Framework Core <c>DbContext</c> registration and JWT bearer
    /// authentication boilerplate (token issuance + a sample protected endpoint).
    /// </summary>
    /// <param name="projectName">The base project name, used to derive the namespace and titles.</param>
    /// <param name="efProvider">The Entity Framework Core provider to wire up, or <see cref="EfCoreProvider.None"/> to skip it.</param>
    /// <param name="dbContextNamespace">The namespace the generated <c>DbContext</c> lives in. Required when <paramref name="efProvider"/> is not <see cref="EfCoreProvider.None"/>.</param>
    /// <param name="includeJwt">When true, adds JWT bearer authentication services, middleware, a token-issuing endpoint, and a sample protected endpoint.</param>
    public static string Generate(string projectName, EfCoreProvider efProvider, string? dbContextNamespace, bool includeJwt)
    {
        var dbContextName = $"{projectName}DbContext";
        var extraUsings = BuildExtraUsings(efProvider, dbContextNamespace, includeJwt);
        var dbContextRegistration = BuildDbContextRegistration(efProvider, dbContextName);
        var authRegistration = BuildAuthRegistration(includeJwt);
        var authMiddleware = BuildAuthMiddleware(includeJwt);
        var authSampleEndpoints = BuildAuthSampleEndpoints(includeJwt);

        return $$"""
        using Serilog;
        {{extraUsings}}

        namespace {{projectName}}_API;

        public class Program
        {
            public static void Main(string[] args)
            {
                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .CreateLogger();

                try
                {
                    Log.Information("Starting {{projectName}} API");

                    var builder = WebApplication.CreateBuilder(args);
                    builder.Host.UseSerilog();

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
        {{dbContextRegistration}}
        {{authRegistration}}
                    var app = builder.Build();

                    // 3. Structured request logging
                    app.UseSerilogRequestLogging();

                    // 4. Configure Middleware Pipeline
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
        {{authMiddleware}}
                    // 5. Working Sample Endpoints
                    app.MapGet("/api/health", () => Results.Ok(new
                    {
                        Status = "Healthy",
                        Project = "{{projectName}} API",
                        Timestamp = DateTime.UtcNow
                    }))
                    .WithName("HealthCheck");
        {{authSampleEndpoints}}
                    app.Run();
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "{{projectName}} API terminated unexpectedly");
                }
                finally
                {
                    Log.CloseAndFlush();
                }
            }
        }
        """;
    }

    private static string BuildExtraUsings(EfCoreProvider efProvider, string? dbContextNamespace, bool includeJwt)
    {
        var usings = new List<string>();

        if (efProvider != EfCoreProvider.None && dbContextNamespace is not null)
        {
            usings.Add("using Microsoft.EntityFrameworkCore;");
            usings.Add($"using {dbContextNamespace};");
        }

        if (includeJwt)
        {
            usings.Add("using Microsoft.AspNetCore.Authentication.JwtBearer;");
            usings.Add("using Microsoft.IdentityModel.Tokens;");
            usings.Add("using System.IdentityModel.Tokens.Jwt;");
            usings.Add("using System.Security.Claims;");
            usings.Add("using System.Text;");
        }

        return usings.Count == 0 ? "" : string.Join("\n", usings);
    }

    private static string BuildDbContextRegistration(EfCoreProvider efProvider, string dbContextName)
    {
        if (efProvider == EfCoreProvider.None)
        {
            return "";
        }

        var useMethod = EfCoreTemplate.UseProviderMethod(efProvider);
        return $$"""

                    // 2b. Entity Framework Core
                    builder.Services.AddDbContext<{{dbContextName}}>(options =>
                        options.{{useMethod}}(builder.Configuration.GetConnectionString("DefaultConnection")));
        """;
    }

    private static string BuildAuthRegistration(bool includeJwt)
    {
        if (!includeJwt)
        {
            return "";
        }

        return """

                    // 2c. JWT Bearer Authentication
                    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                        .AddJwtBearer(options =>
                        {
                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidateIssuer = true,
                                ValidateAudience = true,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,
                                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                                ValidAudience = builder.Configuration["Jwt:Audience"],
                                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                            };
                        });
                    builder.Services.AddAuthorization();
        """;
    }

    private static string BuildAuthMiddleware(bool includeJwt) => includeJwt
        ? "\n                    app.UseAuthentication();\n                    app.UseAuthorization();\n"
        : "";

    private static string BuildAuthSampleEndpoints(bool includeJwt)
    {
        if (!includeJwt)
        {
            return "";
        }

        return """

                    app.MapPost("/api/auth/token", (string username) =>
                    {
                        var claims = new[] { new Claim(ClaimTypes.Name, username) };
                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!));
                        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        var expiryMinutes = builder.Configuration.GetValue<int>("Jwt:ExpiryMinutes", 60);

                        var token = new JwtSecurityToken(
                            issuer: builder.Configuration["Jwt:Issuer"],
                            audience: builder.Configuration["Jwt:Audience"],
                            claims: claims,
                            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                            signingCredentials: credentials);

                        return Results.Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
                    })
                    .WithName("IssueToken");

                    app.MapGet("/api/secure", () => Results.Ok(new { Message = "You are authenticated!" }))
                        .WithName("SecureSample")
                        .RequireAuthorization();
        """;
    }
}
