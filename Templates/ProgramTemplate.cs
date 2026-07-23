using BuildQuickPkg.Scaffolding;

namespace BuildQuickPkg.Templates;

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
        var swaggerJwtSecurity = BuildSwaggerJwtSecurity(includeJwt);
        var dbContextRegistration = BuildDbContextRegistration(efProvider, dbContextName);
        var authRegistration = BuildAuthRegistration(includeJwt);
        var authMiddleware = BuildAuthMiddleware(includeJwt);
        var authSampleEndpoints = BuildAuthSampleEndpoints(includeJwt);

        return $$"""
        using Serilog;
        // BuildQuickPkg:usings
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
                        options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
                        {
                            Title = "{{projectName}} API",
                            Version = "v1",
                            Description = "Clean Architecture Web API scaffolding"
                        });
                        // BuildQuickPkg:swagger
        {{swaggerJwtSecurity}}
                    });
        {{dbContextRegistration}}
        {{authRegistration}}
                    // BuildQuickPkg:services
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

                        // "AllowAll" is convenient for local development only. Before deploying,
                        // replace it with a named policy that allowlists your actual client origin(s).
                        app.UseCors("AllowAll");
                    }

                    if (app.Environment.IsProduction())
                    {
                        app.UseHttpsRedirection();
                    }
        {{authMiddleware}}
                    // BuildQuickPkg:middleware
                    // 5. Working Sample Endpoints
                    app.MapGet("/api/health", () => Results.Ok(new
                    {
                        Status = "Healthy",
                        Project = "{{projectName}} API",
                        Timestamp = DateTime.UtcNow
                    }))
                    .WithName("HealthCheck");
        {{authSampleEndpoints}}
                    // BuildQuickPkg:endpoints
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

    /// <summary>Builds the extra <c>using</c> directives needed for <paramref name="efProvider"/> and/or JWT, reused by <c>BuildQuickPkg add</c> to patch an existing Program.cs.</summary>
    public static string BuildExtraUsings(EfCoreProvider efProvider, string? dbContextNamespace, bool includeJwt)
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

    /// <summary>
    /// Builds the Swagger UI JWT bearer security scheme: this is what makes the "Authorize" button
    /// and the per-endpoint lock icons actually appear, and lets you paste a token into Swagger UI
    /// and have it sent on every request. Without this, JWT bearer auth still works against the API
    /// directly, but Swagger UI has no way to know about it or to let you supply a token.
    /// Reused by <c>BuildQuickPkg add jwt</c> to patch an existing Program.cs.
    /// </summary>
    public static string BuildSwaggerJwtSecurity(bool includeJwt)
    {
        if (!includeJwt)
        {
            return "";
        }

        return """

                    const string securityScheme = "Bearer";
                    options.AddSecurityDefinition(securityScheme, new Microsoft.OpenApi.OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = Microsoft.OpenApi.ParameterLocation.Header,
                        Description = "Paste the token from POST /api/auth/token here (no \"Bearer \" prefix needed)."
                    });
                    options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
                    {
                        [new Microsoft.OpenApi.OpenApiSecuritySchemeReference(securityScheme, document)] = new List<string>()
                    });
        """;
    }

    /// <summary>Builds the <c>AddDbContext</c> registration block, reused by <c>BuildQuickPkg add efcore</c> to patch an existing Program.cs.</summary>
    public static string BuildDbContextRegistration(EfCoreProvider efProvider, string dbContextName)
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

    /// <summary>Builds the JWT bearer authentication service registration, reused by <c>BuildQuickPkg add jwt</c> to patch an existing Program.cs.</summary>
    public static string BuildAuthRegistration(bool includeJwt)
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

    /// <summary>Builds the <c>UseAuthentication</c>/<c>UseAuthorization</c> middleware lines, reused by <c>BuildQuickPkg add jwt</c> to patch an existing Program.cs.</summary>
    public static string BuildAuthMiddleware(bool includeJwt) => includeJwt
        ? "\n                    app.UseAuthentication();\n                    app.UseAuthorization();\n"
        : "";

    /// <summary>Builds the sample token-issuing and protected endpoints, reused by <c>BuildQuickPkg add jwt</c> to patch an existing Program.cs.</summary>
    public static string BuildAuthSampleEndpoints(bool includeJwt)
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
