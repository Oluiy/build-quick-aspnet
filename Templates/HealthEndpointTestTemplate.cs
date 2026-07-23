namespace create_aspnet_app.Templates;

/// <summary>
/// Produces an xUnit integration test exercising the generated API's health-check endpoint.
/// </summary>
internal static class HealthEndpointTestTemplate
{
    /// <summary>Builds a test that boots the API in-memory via <c>WebApplicationFactory</c> and asserts <c>/api/health</c> returns 200 with a healthy status.</summary>
    public static string Generate(string projectName) => $$"""
        using System.Net;
        using System.Net.Http.Json;
        using Microsoft.AspNetCore.Mvc.Testing;
        using Xunit;
        using {{projectName}}_API;

        namespace {{projectName}}.Tests;

        public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
        {
            private readonly HttpClient _client;

            public HealthEndpointTests(WebApplicationFactory<Program> factory)
            {
                _client = factory.CreateClient();
            }

            [Fact]
            public async Task GetHealth_ReturnsOk_WithHealthyStatus()
            {
                var response = await _client.GetAsync("/api/health");

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var content = await response.Content.ReadFromJsonAsync<HealthResponse>();
                Assert.NotNull(content);
                Assert.Equal("Healthy", content!.Status);
            }

            private record HealthResponse(string Status, string Project, DateTime Timestamp);
        }
        """;
}
