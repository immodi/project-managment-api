using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Common;
using Application.DTOs.Auth;
using Application.DTOs.Projects;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

namespace Tests.Integration;

public class ProjectFlowTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<string> RegisterAndGetTokenAsync()
    {
        var email = $"user_{Guid.NewGuid()}@test.com";

        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = email,
            Password = "Password123!"
        });

        response.EnsureSuccessStatusCode();

        var body = await response.Content
            .ReadFromJsonAsync<ApiResponse<AuthResponse>>();

        return body!.Data!.Token;
    }

    private HttpClient AuthorizedClient(string token)
    {
        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    [Fact]
    public async Task Should_create_and_retrieve_project()
    {
        var token = await RegisterAndGetTokenAsync();
        var client = AuthorizedClient(token);

        var createResponse = await client.PostAsJsonAsync("/api/projects", new
        {
            Name = "Integration Project",
            Description = "Created in integration test"
        }, cancellationToken: TestContext.Current.CancellationToken);

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var created = await createResponse.Content
            .ReadFromJsonAsync<ApiResponse<ProjectResponse>>(cancellationToken: TestContext.Current.CancellationToken);

        created!.Data!.Name.Should().Be("Integration Project");
        created.Data.Id.Should().NotBeEmpty();

        var getResponse = await client.GetAsync($"/api/projects/{created.Data.Id}", TestContext.Current.CancellationToken);

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetched = await getResponse.Content
            .ReadFromJsonAsync<ApiResponse<ProjectResponse>>(cancellationToken: TestContext.Current.CancellationToken);

        fetched!.Data!.Id.Should().Be(created.Data.Id);
        fetched.Data.Name.Should().Be("Integration Project");
    }

    [Fact]
    public async Task Should_list_only_own_projects()
    {
        var token = await RegisterAndGetTokenAsync();
        var client = AuthorizedClient(token);

        await client.PostAsJsonAsync("/api/projects", new { Name = "Project A", Description = "A" }, cancellationToken: TestContext.Current.CancellationToken);

        await client.PostAsJsonAsync("/api/projects", new { Name = "Project B", Description = "B" }, cancellationToken: TestContext.Current.CancellationToken);

        var listResponse = await client.GetAsync("/api/projects", TestContext.Current.CancellationToken);

        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await listResponse.Content
            .ReadFromJsonAsync<ApiResponse<List<ProjectResponse>>>(cancellationToken: TestContext.Current.CancellationToken);

        list!.Data.Should().Contain(p => p.Name == "Project A");
        list.Data.Should().Contain(p => p.Name == "Project B");
    }

    [Fact]
    public async Task Should_update_project()
    {
        var token = await RegisterAndGetTokenAsync();
        var client = AuthorizedClient(token);

        var created = await (await client.PostAsJsonAsync("/api/projects", new { Name = "Before Update", Description = "Old" }, cancellationToken: TestContext.Current.CancellationToken))
            .Content.ReadFromJsonAsync<ApiResponse<ProjectResponse>>(cancellationToken: TestContext.Current.CancellationToken);

        var updateResponse = await client.PutAsJsonAsync($"/api/projects/{created!.Data!.Id}", new { Name = "After Update", Description = "New" }, cancellationToken: TestContext.Current.CancellationToken);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var fetched = await (await client.GetAsync($"/api/projects/{created.Data.Id}", TestContext.Current.CancellationToken))
            .Content.ReadFromJsonAsync<ApiResponse<ProjectResponse>>(cancellationToken: TestContext.Current.CancellationToken);

        fetched!.Data!.Name.Should().Be("After Update");
    }

    [Fact]
    public async Task Should_delete_project()
    {
        var token = await RegisterAndGetTokenAsync();
        var client = AuthorizedClient(token);

        var created = await (await client.PostAsJsonAsync("/api/projects", new { Name = "To Delete", Description = "Will be gone" }, cancellationToken: TestContext.Current.CancellationToken))
            .Content.ReadFromJsonAsync<ApiResponse<ProjectResponse>>(cancellationToken: TestContext.Current.CancellationToken);

        var deleteResponse = await client.DeleteAsync($"/api/projects/{created!.Data!.Id}", TestContext.Current.CancellationToken);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/projects/{created.Data.Id}", TestContext.Current.CancellationToken);

        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_return_401_when_not_authenticated()
    {
        var response = await _client.GetAsync("/api/projects", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_return_404_when_accessing_another_users_project()
    {
        var token1 = await RegisterAndGetTokenAsync();
        var token2 = await RegisterAndGetTokenAsync();

        var client1 = AuthorizedClient(token1);
        var client2 = AuthorizedClient(token2);

        var created = await (await client1.PostAsJsonAsync("/api/projects", new { Name = "User1 Project", Description = "Private" }, cancellationToken: TestContext.Current.CancellationToken))
            .Content.ReadFromJsonAsync<ApiResponse<ProjectResponse>>(cancellationToken: TestContext.Current.CancellationToken);

        var response = await client2.GetAsync($"/api/projects/{created!.Data!.Id}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}