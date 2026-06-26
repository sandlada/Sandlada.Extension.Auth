using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Sandlada.Extension.Auth.Api.Tests;

[Collection("Auth API Tests")]
public sealed class GraphQLMutationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public GraphQLMutationTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseContentRoot(Directory.GetCurrentDirectory());
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = $"Data Source=test-gql-{Guid.NewGuid():N}.db",
                    ["Email:Smtp:Enabled"] = "false",
                });
            });
        });
    }

    [Fact]
    public async Task LoginByEmailAndPassword_InvalidCredentials_ReturnsError()
    {
        var client = this.factory.CreateClient();

        var response = await client.PostAsJsonAsync("/graphql", new
        {
            query = """
                mutation {
                    loginByEmailAddressAndPassword(input: {
                        emailAddress: "nonexistent@example.com"
                        password: "wrong"
                    }) {
                        userId
                        emailAddress
                    }
                }
                """
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("errors", out var errors), "Expected GraphQL errors for invalid login");
    }

    [Fact]
    public async Task RequestRegistrationVerificationCode_ReturnsError_WhenEmailOnly()
    {
        var client = this.factory.CreateClient();

        var response = await client.PostAsJsonAsync("/graphql", new
        {
            query = """
                mutation {
                    requestRegistrationVerificationCode(input: {
                        emailAddress: "test@example.com"
                    }) {
                        expiresAt
                    }
                }
                """
        });

        // Password is a required field in the schema; omitting it causes a
        // GraphQL validation error (HTTP 400) before the resolver runs.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("errors", out _), "Expected GraphQL errors when only email provided");
    }

    [Fact]
    public async Task LoginByEmailAndVerificationCode_ReturnsError_InvalidCredentials()
    {
        var client = this.factory.CreateClient();

        var response = await client.PostAsJsonAsync("/graphql", new
        {
            query = """
                mutation {
                    loginByEmailAddressAndVerificationCode(input: {
                        emailAddress: "nonexistent@example.com"
                        verificationCode: "000000"
                    }) {
                        userId
                        emailAddress
                    }
                }
                """
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("errors", out _), "Expected GraphQL errors for invalid login");
    }

    [Fact]
    public async Task RequestLoginVerificationCode_ReturnsSuccess_ForExistingUser()
    {
        var client = this.factory.CreateClient();

        // The dev seed creates user@example.com, so requesting a login code should succeed
        var response = await client.PostAsJsonAsync("/graphql", new
        {
            query = """
                mutation {
                    requestLoginVerificationCode(input: {
                        emailAddress: "user@example.com"
                    }) {
                        expiresAt
                    }
                }
                """
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        // Should succeed with no errors (requesting code doesn't require password)
        Assert.False(json.TryGetProperty("errors", out _), "Expected success for existing user login code request");
        Assert.True(json.TryGetProperty("data", out var data), "Expected data in response");
        Assert.NotNull(data.GetProperty("requestLoginVerificationCode"));
    }

    [Fact]
    public async Task GetCurrentUser_WithoutAuth_ReturnsUnauthorized()
    {
        var client = this.factory.CreateClient();

        var response = await client.PostAsJsonAsync("/graphql", new
        {
            query = """
                query {
                    currentUser {
                        userId
                        emailAddress
                    }
                }
                """
        });

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Expected OK but got {response.StatusCode}. Body: {body}");
        }

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("errors", out var errors), "Expected GraphQL errors for unauthorized query");
    }

    [Fact]
    public async Task GetCurrentUserProfile_WithoutAuth_ReturnsUnauthorized()
    {
        var client = this.factory.CreateClient();

        var response = await client.PostAsJsonAsync("/graphql", new
        {
            query = """
                query {
                    currentUserProfile {
                        id
                        userId
                    }
                }
                """
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("errors", out _), "Expected GraphQL errors for unauthorized query");
    }

    [Fact]
    public async Task GetCurrentUserStatus_WithoutAuth_ReturnsUnauthorized()
    {
        var client = this.factory.CreateClient();

        var response = await client.PostAsJsonAsync("/graphql", new
        {
            query = """
                query {
                    currentUserStatus {
                        status
                    }
                }
                """
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("errors", out _), "Expected GraphQL errors for unauthorized query");
    }

    [Fact]
    public async Task Logout_WithoutAuth_ReturnsUnauthorized()
    {
        var client = this.factory.CreateClient();

        var response = await client.PostAsJsonAsync("/graphql", new
        {
            query = """
                mutation {
                    logout
                }
                """
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("errors", out _), "Expected GraphQL errors for unauthorized logout");
    }

    [Fact]
    public async Task InsertCurrentUserProfile_WithoutAuth_ReturnsUnauthorized()
    {
        var client = this.factory.CreateClient();

        var response = await client.PostAsJsonAsync("/graphql", new
        {
            query = """
                mutation {
                    insertCurrentUserProfile(input: {
                        displayName: "Test"
                    }) {
                        id
                        userId
                    }
                }
                """
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("errors", out _), "Expected GraphQL errors for unauthorized mutation");
    }

    [Fact]
    public async Task InsertOrUpdateCurrentUserProfile_WithoutAuth_ReturnsUnauthorized()
    {
        var client = this.factory.CreateClient();

        var response = await client.PostAsJsonAsync("/graphql", new
        {
            query = """
                mutation {
                    insertOrUpdateCurrentUserProfile(input: {
                        displayName: "Test"
                    }) {
                        id
                        userId
                    }
                }
                """
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("errors", out _), "Expected GraphQL errors for unauthorized mutation");
    }

    [Fact]
    public async Task ResetOneCurrentUserProfile_WithoutAuth_ReturnsUnauthorized()
    {
        var client = this.factory.CreateClient();

        var response = await client.PostAsJsonAsync("/graphql", new
        {
            query = """
                mutation {
                    resetOneCurrentUserProfile {
                        id
                        userId
                    }
                }
                """
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("errors", out _), "Expected GraphQL errors for unauthorized mutation");
    }

    [Fact]
    public async Task RequestEmailRebindVerificationCode_WithoutAuth_ReturnsUnauthorized()
    {
        var client = this.factory.CreateClient();

        var response = await client.PostAsJsonAsync("/graphql", new
        {
            query = """
                mutation {
                    requestEmailRebindVerificationCode(input: {
                        emailAddress: "newemail@example.com"
                    }) {
                        emailAddress
                        expiresAt
                    }
                }
                """
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("errors", out _), "Expected GraphQL errors for unauthorized mutation");
    }

    [Fact]
    public async Task ConfirmEmailRebind_WithoutAuth_ReturnsUnauthorized()
    {
        var client = this.factory.CreateClient();

        var response = await client.PostAsJsonAsync("/graphql", new
        {
            query = """
                mutation {
                    confirmEmailRebind(input: {
                        emailAddress: "newemail@example.com"
                        verificationCode: "123456"
                    }) {
                        userId
                        emailAddress
                    }
                }
                """
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("errors", out _), "Expected GraphQL errors for unauthorized mutation");
    }

    [Fact]
    public async Task GetUserById_WithoutAuth_ReturnsNull()
    {
        var client = this.factory.CreateClient();

        var response = await client.PostAsJsonAsync("/graphql", new
        {
            query = """
                query {
                    userById(userId: "00000000-0000-0000-0000-000000000000") {
                        userId
                        emailAddress
                    }
                }
                """
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        // Admin queries return null/[] for non-admin, NOT GraphQL errors
        Assert.True(json.TryGetProperty("data", out var data), "Expected data in response");
        Assert.Equal(JsonValueKind.Null, data.GetProperty("userById").ValueKind);
    }

    [Fact]
    public async Task GetOAuthClients_WithoutAuth_ReturnsEmptyArray()
    {
        var client = this.factory.CreateClient();

        var response = await client.PostAsJsonAsync("/graphql", new
        {
            query = """
                query {
                    oAuthClients {
                        clientId
                    }
                }
                """
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("data", out var data), "Expected data in response");
        var clients = data.GetProperty("oAuthClients");
        Assert.Equal(JsonValueKind.Array, clients.ValueKind);
        Assert.Empty(clients.EnumerateArray());
    }
}
