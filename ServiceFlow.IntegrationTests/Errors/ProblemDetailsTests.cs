using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ServiceFlow.IntegrationTests.Errors
{
    public class ProblemDetailsTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ProblemDetailsTests(WebApplicationFactory<Program> factory) =>
            _client = factory.WithWebHostBuilder(b => b.ConfigureTestServices(s =>
                s.AddControllers().AddApplicationPart(typeof(ThrowingController).Assembly))).CreateClient();

        [Theory]
        [InlineData("validation", 400)]
        [InlineData("notfound", 404)]
        [InlineData("conflict", 409)]
        [InlineData("concurrency", 409)]
        [InlineData("domain", 422)]
        [InlineData("boom", 500)]
        public async Task Controlled_errors_use_problem_json(string kind, int status)
        {
            var res = await _client.GetAsync($"/test-errors/{kind}");

            Assert.Equal(status, (int)res.StatusCode);
            Assert.Equal("application/problem+json", res.Content.Headers.ContentType!.MediaType);

            var body = await res.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(status, body.GetProperty("status").GetInt32());
            Assert.StartsWith("https://api.serviceflow.com/errors/", body.GetProperty("type").GetString());
            Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("title").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("detail").GetString()));

            Assert.True(body.TryGetProperty("errors", out var errors));
            Assert.Equal(JsonValueKind.Object, errors.ValueKind);
        }

        [Fact]
        public async Task Validation_error_includes_errors()
        {
            var res = await _client.GetAsync("/test-errors/validation");
            var body = await res.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal("The email address is invalid.",
                body.GetProperty("errors").GetProperty("Email")[0].GetString());
        }

        [Fact]
        public async Task Model_binding_failure_returns_400_with_errors()
        {
            var res = await _client.PostAsJsonAsync("/test-errors/model", new { });
            var body = await res.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(400, (int)res.StatusCode);
            Assert.True(body.TryGetProperty("errors", out _));
            Assert.StartsWith("https://api.serviceflow.com/errors/validation", body.GetProperty("type").GetString());
        }

        [Fact]
        public async Task Unknown_route_returns_404_with_empty_errors()
        {
            var res = await _client.GetAsync("/no-existe");
            var body = await res.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(404, (int)res.StatusCode);
            Assert.Equal("application/problem+json", res.Content.Headers.ContentType!.MediaType);
            Assert.Equal(JsonValueKind.Object, body.GetProperty("errors").ValueKind);
            Assert.Empty(body.GetProperty("errors").EnumerateObject());
        }

        [Fact]
        public async Task Unexpected_error_does_not_leak_internal_message()
        {
            var res = await _client.GetAsync("/test-errors/boom");
            var text = await res.Content.ReadAsStringAsync();

            Assert.DoesNotContain("secreto interno", text);
        }

        [Fact]
        public async Task Unknown_route_returns_404_with_filled_errors_and_detail()
        {
            var res = await _client.GetAsync("/no-existe");
            var body = await res.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(404, (int)res.StatusCode);
            Assert.Equal("application/problem+json", res.Content.Headers.ContentType!.MediaType);
            Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("detail").GetString()));
            Assert.NotEmpty(body.GetProperty("errors").EnumerateObject());
            Assert.Equal("The requested resource was not found.",
                body.GetProperty("errors").GetProperty("NotFound")[0].GetString());
        }
    }
}
