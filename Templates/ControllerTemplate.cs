namespace BuildQuickPkg.Templates;

/// <summary>
/// Produces the Controller + service pair generated when <see cref="Scaffolding.ApiStyle.Controller"/>
/// is selected: a Controller in the API project's <c>Controllers/</c> folder, backed by an
/// interface and implementation in the Application project's <c>Services/Interfaces/</c> and
/// <c>Services/Implementation/</c> folders (the service-controller pattern). Every route uses an
/// explicit full path on the <c>[Http*]</c> attribute rather than <c>[Route("api/[controller]")]</c>,
/// so the URLs are identical to the Minimal API equivalents in <see cref="ProgramTemplate"/> -
/// callers, docs, and generated tests don't need to know which style a project was generated with.
/// </summary>
internal static class ControllerTemplate
{
    public static string HealthController(string projectName) => $$"""
        using Microsoft.AspNetCore.Mvc;
        using {{projectName}}_Application.Services.Interfaces;

        namespace {{projectName}}_API.Controllers;

        [ApiController]
        public class HealthController : ControllerBase
        {
            private readonly IHealthService _healthService;

            public HealthController(IHealthService healthService)
            {
                _healthService = healthService;
            }

            [HttpGet("api/health")]
            public IActionResult Get() => Ok(_healthService.GetHealth());
        }
        """;

    public static string IHealthService(string projectName) => $$"""
        namespace {{projectName}}_Application.Services.Interfaces;

        public interface IHealthService
        {
            object GetHealth();
        }
        """;

    public static string HealthService(string projectName) => $$"""
        using {{projectName}}_Application.Services.Interfaces;

        namespace {{projectName}}_Application.Services.Implementation;

        public class HealthService : IHealthService
        {
            public object GetHealth() => new
            {
                Status = "Healthy",
                Project = "{{projectName}} API",
                Timestamp = DateTime.UtcNow
            };
        }
        """;

    public static string AuthController(string projectName) => $$"""
        using Microsoft.AspNetCore.Authorization;
        using Microsoft.AspNetCore.Mvc;
        using {{projectName}}_Application.Services.Interfaces;

        namespace {{projectName}}_API.Controllers;

        [ApiController]
        public class AuthController : ControllerBase
        {
            private readonly IAuthService _authService;

            public AuthController(IAuthService authService)
            {
                _authService = authService;
            }

            [HttpPost("api/auth/token")]
            public IActionResult Token([FromQuery] string username) =>
                Ok(new { Token = _authService.IssueToken(username) });

            [HttpGet("api/secure")]
            [Authorize]
            public IActionResult Secure() => Ok(new { Message = "You are authenticated!" });
        }
        """;

    public static string IAuthService(string projectName) => $$"""
        namespace {{projectName}}_Application.Services.Interfaces;

        public interface IAuthService
        {
            string IssueToken(string username);
        }
        """;

    public static string AuthService(string projectName) => $$"""
        using System.IdentityModel.Tokens.Jwt;
        using System.Security.Claims;
        using System.Text;
        using Microsoft.Extensions.Configuration;
        using Microsoft.IdentityModel.Tokens;
        using {{projectName}}_Application.Services.Interfaces;

        namespace {{projectName}}_Application.Services.Implementation;

        public class AuthService : IAuthService
        {
            private readonly IConfiguration _configuration;

            public AuthService(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public string IssueToken(string username)
            {
                var claims = new[] { new Claim(ClaimTypes.Name, username) };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes", 60);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                    signingCredentials: credentials);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
        }
        """;
}
