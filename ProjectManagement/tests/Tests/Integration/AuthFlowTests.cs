using System.Net;
using System.Net.Http.Json;
using Application.Common;
using Application.DTOs.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

namespace Tests.Integration;

public class AuthFlowTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Should_register_and_login_successfully()
    {
        var email = $"user_{Guid.NewGuid()}@test.com";
        var password = "Password123!";

        // REGISTER
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = email,
            Password = password
        }, cancellationToken: TestContext.Current.CancellationToken);

        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var registerResult = await registerResponse.Content
            .ReadFromJsonAsync<ApiResponse<AuthResponse>>(cancellationToken: TestContext.Current.CancellationToken);

        registerResult!.Data!.Token.Should().NotBeNullOrWhiteSpace();
        
        // LOGIN
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = password
        }, cancellationToken: TestContext.Current.CancellationToken);

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResult = await loginResponse.Content
            .ReadFromJsonAsync<ApiResponse<AuthResponse>>(cancellationToken: TestContext.Current.CancellationToken);

        loginResult!.Data!.Token.Should().NotBeNullOrWhiteSpace();
    }
}