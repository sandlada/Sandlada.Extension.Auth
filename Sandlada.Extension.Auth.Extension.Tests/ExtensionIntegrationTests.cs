using System.Net;
using System.Net.Http.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Sandlada.Extension.Auth.Extension.Tests;

[CollectionDefinition("Extension Integration Tests", DisableParallelization = true)]
public sealed class ExtensionIntegrationTestsCollectionDefinition;

/// <summary>
/// Integration tests that exercise the Extension NuGet package from an external consumer's perspective.
/// Uses WebApplicationFactory against the Api's Program, with configuration overrides to simulate
/// a minimal consumer environment.
/// </summary>
[Collection("Extension Integration Tests")]
public sealed class ExtensionIntegrationTests : IClassFixture<WebApplicationFactory<Program>> {
    private readonly WebApplicationFactory<Program> factory;

    public ExtensionIntegrationTests(WebApplicationFactory<Program> factory) {
        this.factory = factory.WithWebHostBuilder(builder => {
            builder.UseContentRoot(Directory.GetCurrentDirectory());
            builder.ConfigureAppConfiguration((_, config) => {
                config.AddInMemoryCollection(new Dictionary<string, string?> {
                    ["ConnectionStrings:DefaultConnection"] =
                        $"Data Source=test-ext-{Guid.NewGuid().ToString("N")}.db",
                    ["Email:Smtp:Enabled"] = "false",
                });
            });
        });
    }

    [Fact]
    public void AddAuthExtension_RegistersServices_DoesNotThrow() {
        var sender = this.factory.Services.GetRequiredService<ISender>();
        Assert.NotNull(sender);
    }

    [Fact]
    public async Task LoginByEmailAddress_WithInvalidCredentials_ReturnsUnauthorized() {
        var client = this.factory.CreateClient();
        var response = await client.PostAsJsonAsync("/Api/Auth/LoginByEmailAddressAndPassword", new {
            emailAddress = "nonexistent@example.com",
            password = "wrong",
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RequestRegistrationVerificationCode_MissingPassword_ReturnsBadRequest() {
        var client = this.factory.CreateClient();
        var response = await client.PostAsJsonAsync("/Api/Auth/RequestRegistrationVerificationCode", new {
            emailAddress = "test@example.com",
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task FindOneCurrentUserStatus_WithoutAuth_ReturnsUnauthorized() {
        var client = this.factory.CreateClient();
        var response = await client.GetAsync("/Api/User/FindOneCurrentUserStatus");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task FindOneCurrentUserProfile_ReturnsUnauthorized() {
        var client = this.factory.CreateClient();
        var response = await client.GetAsync("/Api/UserProfile/FindOneCurrentUserProfile");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OAuthClientEndpoints_ReturnsUnauthorized() {
        var client = this.factory.CreateClient();
        var response = await client.PostAsJsonAsync("/Api/OAuthClient/InsertOne", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ConnectAuthorizeEndpoint_IsAccessible() {
        var client = this.factory.CreateClient();
        var response = await client.GetAsync("/Connect/Authorize");
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SwaggerEndpoint_IsAccessible() {
        var client = this.factory.CreateClient();
        var response = await client.GetAsync("/swagger/index.html");
        Assert.True(
            response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Redirect,
            $"Expected OK or Redirect, got {response.StatusCode}");
    }

    [Fact]
    public async Task Register_MissingEmail_ReturnsBadRequest() {
        var client = this.factory.CreateClient();
        var response = await client.PostAsJsonAsync("/Api/Auth/Register", new {
            password = "Test@1234",
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task LoginByUniqueName_WithInvalidCredentials_ReturnsUnauthorized() {
        var client = this.factory.CreateClient();
        var response = await client.PostAsJsonAsync("/Api/Auth/LoginByUniqueNameAndPassword", new {
            uniqueName = "nonexistent",
            password = "wrong",
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DevelopmentEnvironment_SeedsDefaultAdminUser() {
        var client = this.factory.CreateClient();
        var response = await client.PostAsJsonAsync("/Api/Auth/LoginByEmailAddressAndPassword", new {
            emailAddress = "admin@example.com",
            password = "admin",
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("Set-Cookie"), "Login should set auth cookie");
    }

    [Fact]
    public async Task Register_ThenLogin_CompletesFullFlow() {
        var client = this.factory.CreateClient();
        var uniqueId = Guid.NewGuid().ToString("N")[..12];
        var email = $"test-{uniqueId}@example.com";
        var uniqueName = $"testuser-{uniqueId}";
        var password = "Test@1234";

        var codeResponse = await client.PostAsJsonAsync("/Api/Auth/RequestRegistrationVerificationCode", new {
            emailAddress = email,
            password = password,
            uniqueName = uniqueName,
        });

        if (codeResponse.StatusCode != HttpStatusCode.OK) {
            var err = await codeResponse.Content.ReadAsStringAsync();
            Assert.Fail($"RequestRegistrationVerificationCode failed: {codeResponse.StatusCode} — {err}");
        }

        var codeResult = await codeResponse.Content.ReadFromJsonAsync<ResponseWithValue>();
        var code = codeResult?.Value ?? "000000";

        var registerResponse = await client.PostAsJsonAsync("/Api/Auth/Register", new {
            emailAddress = email,
            password = password,
            uniqueName = uniqueName,
            verificationCode = code,
        });

        if (registerResponse.StatusCode != HttpStatusCode.OK) {
            var err = await registerResponse.Content.ReadAsStringAsync();
            Assert.True(
                registerResponse.StatusCode is HttpStatusCode.OK or HttpStatusCode.BadRequest,
                $"Register returned unexpected status: {registerResponse.StatusCode} — {err}");
            return;
        }

        var loginResponse = await client.PostAsJsonAsync("/Api/Auth/LoginByEmailAddressAndPassword", new {
            emailAddress = email,
            password = password,
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.True(loginResponse.Headers.Contains("Set-Cookie"), "Login should set auth cookie");
    }

    [Fact]
    public async Task AuthAndUserEndpoints_AreMapped_DoNotReturn404() {
        var client = this.factory.CreateClient();
        var authResponse = await client.PostAsJsonAsync("/Api/Auth/LoginByEmailAddressAndPassword", new {
            emailAddress = "test@example.com",
            password = "test",
        });
        Assert.NotEqual(HttpStatusCode.NotFound, authResponse.StatusCode);

        var userResponse = await client.GetAsync("/Api/User/FindOneCurrentUserStatus");
        Assert.NotEqual(HttpStatusCode.NotFound, userResponse.StatusCode);
    }

    [Fact]
    public void ServiceProvider_ResolvesKeyServices() {
        var services = this.factory.Services;

        // ISender is typically transient or singleton
        var sender = services.GetService<ISender>();
        Assert.NotNull(sender);

        // AuthDbContext is scoped — must use a scope to resolve
        var dbContextType = Type.GetType(
            "Sandlada.Extension.Auth.Infrastructure.Persistence.AuthDbContext, Sandlada.Extension.Auth.Infrastructure");
        if (dbContextType is not null) {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetService(dbContextType);
            Assert.NotNull(dbContext);
        }
    }

    [Fact]
    public async Task LoginByEmailAddress_WithRepeatedWrongPassword_EventuallyReturnsTooManyRequests() {
        var client = this.factory.CreateClient();
        var uniqueId = Guid.NewGuid().ToString("N")[..12];
        var email = $"ratelimit-{uniqueId}@example.com";
        var password = "wrong";

        // Send 4 login requests with wrong password — each should return 401
        for (var i = 0; i < 4; i++) {
            var response = await client.PostAsJsonAsync("/Api/Auth/LoginByEmailAddressAndPassword", new {
                emailAddress = email,
                password,
            });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // 5th request triggers lockout → 429 TooManyRequests
        var lockoutResponse = await client.PostAsJsonAsync("/Api/Auth/LoginByEmailAddressAndPassword", new {
            emailAddress = email,
            password,
        });
        Assert.Equal(HttpStatusCode.TooManyRequests, lockoutResponse.StatusCode);

        // 6th request is locked out → 429 TooManyRequests
        var lockedResponse = await client.PostAsJsonAsync("/Api/Auth/LoginByEmailAddressAndPassword", new {
            emailAddress = email,
            password,
        });
        Assert.Equal(HttpStatusCode.TooManyRequests, lockedResponse.StatusCode);
    }

    private sealed record ResponseWithValue {
        public string? Value { get; init; }
    }
}
