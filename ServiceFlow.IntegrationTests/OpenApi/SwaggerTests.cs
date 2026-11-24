using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ServiceFlow.IntegrationTests.OpenApi
{
    public class SwaggerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public SwaggerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Development"));
        }

        [Fact]
        public async Task Swagger_ui_is_served()
        {
            using var client = _factory.CreateClient();

            using var response = await client.GetAsync("/swagger");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);

            var html = await response.Content.ReadAsStringAsync();
            Assert.Contains("swagger-ui", html, StringComparison.OrdinalIgnoreCase);

            // Swashbuckle 11 mueve la configuración de la UI a /swagger/index.js.
            var config = await client.GetStringAsync("/swagger/index.js");
            Assert.Contains("swagger/v1/swagger.json", config);
        }

        [Fact]
        public async Task Openapi_document_is_served()
        {
            var document = await GetDocumentAsync();

            Assert.Equal("v1", document.GetProperty("info").GetProperty("version").GetString());
            Assert.Equal("ServiceFlow API", document.GetProperty("info").GetProperty("title").GetString());
        }

        [Fact]
        public async Task Document_describes_only_v1_routes()
        {
            var document = await GetDocumentAsync();
            var paths = document.GetProperty("paths");

            Assert.True(HasPath(paths, "/api/v1/WeatherForecast"));
            Assert.False(HasPath(paths, "/WeatherForecast"));
        }

        [Fact]
        public async Task Document_declares_bearer_security_scheme()
        {
            var document = await GetDocumentAsync();

            var scheme = document.GetProperty("components")
                .GetProperty("securitySchemes")
                .GetProperty("Bearer");

            Assert.Equal("http", scheme.GetProperty("type").GetString());
            Assert.Equal("bearer", scheme.GetProperty("scheme").GetString());
            Assert.Equal("JWT", scheme.GetProperty("bearerFormat").GetString());

            var requirements = document.GetProperty("security").EnumerateArray();
            Assert.Contains(requirements, requirement => requirement.TryGetProperty("Bearer", out _));
        }

        [Fact]
        public async Task Versioned_endpoint_returns_ok()
        {
            using var client = _factory.CreateClient();

            using var response = await client.GetAsync("/api/v1/WeatherForecast");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = (await response.Content.ReadFromJsonAsync<JsonElement>())!;
            Assert.Equal(JsonValueKind.Array, body.ValueKind);
            Assert.NotEmpty(body.EnumerateArray());
        }

        [Fact]
        public async Task Unversioned_endpoint_is_not_available()
        {
            using var client = _factory.CreateClient();

            using var response = await client.GetAsync("/WeatherForecast");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Swagger_is_exposed_outside_development()
        {
            using var factory = _factory.WithWebHostBuilder(builder => builder.UseEnvironment("Production"));
            using var client = factory.CreateClient();

            using var documentResponse = await client.GetAsync("/swagger/v1/swagger.json");
            using var uiResponse = await client.GetAsync("/swagger");

            Assert.Equal(HttpStatusCode.OK, documentResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, uiResponse.StatusCode);
        }

        private async Task<JsonElement> GetDocumentAsync()
        {
            using var client = _factory.CreateClient();

            using var response = await client.GetAsync("/swagger/v1/swagger.json");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

            return (await response.Content.ReadFromJsonAsync<JsonElement>())!;
        }

        private static bool HasPath(JsonElement paths, string expected)
        {
            foreach (var path in paths.EnumerateObject())
            {
                if (string.Equals(path.Name, expected, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
