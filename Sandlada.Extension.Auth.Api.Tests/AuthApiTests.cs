using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Sandlada.Extension.Auth.Api.Tests;

public sealed class AuthApiTests : IClassFixture<WebApplicationFactory<Program>> {
    private readonly WebApplicationFactory<Program> factory;

    public AuthApiTests(WebApplicationFactory<Program> factory) {
        this.factory = factory.WithWebHostBuilder(builder => {
            builder.UseContentRoot(Directory.GetCurrentDirectory());
            builder.ConfigureAppConfiguration((context, config) => {
                config.AddInMemoryCollection(new Dictionary<string, string?> {
                    ["ConnectionStrings:DefaultConnection"] = "Data Source=test-auth.db",
                });
            });
        });
    }

    [Fact]
    public async Task LoginByEmailAddress_WithInvalidCredentials_ReturnsUnauthorized() {
        var client = this.factory.CreateClient();

        var response = await client.PostAsJsonAsync("/Api/Auth/LoginByEmailAddress", new {
            emailAddress = "nonexistent@example.com",
            password = "wrong",
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegistrationVerificationCode_MissingPassword_ReturnsBadRequest() {
        var client = this.factory.CreateClient();

        var response = await client.PostAsJsonAsync("/Api/Auth/RequestRegistrationVerificationCode", new {
            emailAddress = "test@example.com",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task HealthCheck_GetUserProfiles_ReturnsUnauthorized() {
        var client = this.factory.CreateClient();

        var response = await client.GetAsync("/Api/UserProfile/FindOneCurrentUserProfile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
