using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Common;
using Application.DTOs.Auth;
using Application.DTOs.Projects;
using Application.DTOs.Tasks;
using Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;
using TaskStatus = Domain.Enums.TaskStatus;

namespace Tests.Integration;

public class TaskFlowTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<(string Token, HttpClient Client)> RegisteredClientAsync()
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

        var token = body!.Data!.Token;

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return (token, client);
    }

    private async Task<ProjectResponse> CreateProjectAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/projects",
            new { Name = $"Project_{Guid.NewGuid()}", Description = "Test project" });

        response.EnsureSuccessStatusCode();

        var body = await response.Content
            .ReadFromJsonAsync<ApiResponse<ProjectResponse>>();

        return body!.Data!;
    }

    [Fact]
    public async Task Should_create_and_retrieve_task()
    {
        var (_, client) = await RegisteredClientAsync();
        var project = await CreateProjectAsync(client);

        var createResponse = await client.PostAsJsonAsync("/api/tasks", new
        {
            Title = "Integration Task",
            Description = "Created in test",
            DueDate = DateTime.UtcNow.AddDays(3),
            Priority = (int)TaskPriority.High,
            ProjectId = project.Id
        }, cancellationToken: TestContext.Current.CancellationToken);

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var created = await createResponse.Content
            .ReadFromJsonAsync<ApiResponse<TaskResponse>>(cancellationToken: TestContext.Current.CancellationToken);

        created!.Data!.Title.Should().Be("Integration Task");
        created.Data.ProjectId.Should().Be(project.Id);
        created.Data.Status.Should().Be(TaskStatus.Todo);

        var listResponse = await client.GetAsync($"/api/tasks/project/{project.Id}", TestContext.Current.CancellationToken);

        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var tasks = await listResponse.Content
            .ReadFromJsonAsync<ApiResponse<List<TaskResponse>>>(cancellationToken: TestContext.Current.CancellationToken);

        tasks!.Data.Should().Contain(t => t.Id == created.Data.Id);
    }

    [Fact]
    public async Task Should_update_task_status()
    {
        var (_, client) = await RegisteredClientAsync();
        var project = await CreateProjectAsync(client);

        var created = await (await client.PostAsJsonAsync("/api/tasks", new
        {
            Title = "Status Task",
            Description = "Will change status",
            DueDate = DateTime.UtcNow.AddDays(3),
            Priority = (int)TaskPriority.Medium,
            ProjectId = project.Id
        }, cancellationToken: TestContext.Current.CancellationToken))
            .Content.ReadFromJsonAsync<ApiResponse<TaskResponse>>(cancellationToken: TestContext.Current.CancellationToken);

        var patchResponse = await client.PatchAsJsonAsync($"/api/tasks/{created!.Data!.Id}/status", new { Status = (int)TaskStatus.Done }, cancellationToken: TestContext.Current.CancellationToken);

        patchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var tasks = await (await client.GetAsync($"/api/tasks/project/{project.Id}", TestContext.Current.CancellationToken))
            .Content.ReadFromJsonAsync<ApiResponse<List<TaskResponse>>>(cancellationToken: TestContext.Current.CancellationToken);

        tasks!.Data.Should()
            .Contain(t => t.Id == created.Data.Id && t.Status == TaskStatus.Done);
    }

    [Fact]
    public async Task Should_update_task_fields()
    {
        var (_, client) = await RegisteredClientAsync();
        var project = await CreateProjectAsync(client);

        var created = await (await client.PostAsJsonAsync("/api/tasks", new
        {
            Title = "Original Title",
            Description = "Original Desc",
            DueDate = DateTime.UtcNow.AddDays(3),
            Priority = (int)TaskPriority.Low,
            ProjectId = project.Id
        }, cancellationToken: TestContext.Current.CancellationToken))
            .Content.ReadFromJsonAsync<ApiResponse<TaskResponse>>(cancellationToken: TestContext.Current.CancellationToken);

        var updateResponse = await client.PutAsJsonAsync($"/api/tasks/{created!.Data!.Id}", new
            {
                Title = "Updated Title",
                Description = "Updated Desc",
                DueDate = DateTime.UtcNow.AddDays(5),
                Priority = (int)TaskPriority.High
            }, cancellationToken: TestContext.Current.CancellationToken);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var tasks = await (await client.GetAsync($"/api/tasks/project/{project.Id}", TestContext.Current.CancellationToken))
            .Content.ReadFromJsonAsync<ApiResponse<List<TaskResponse>>>(cancellationToken: TestContext.Current.CancellationToken);

        var updated = tasks!.Data.First(t => t.Id == created.Data.Id);
        updated.Title.Should().Be("Updated Title");
        updated.Priority.Should().Be(TaskPriority.High);
    }

    [Fact]
    public async Task Should_delete_task()
    {
        var (_, client) = await RegisteredClientAsync();
        var project = await CreateProjectAsync(client);

        var created = await (await client.PostAsJsonAsync("/api/tasks", new
        {
            Title = "To Delete",
            Description = "Gone soon",
            DueDate = DateTime.UtcNow.AddDays(3),
            Priority = (int)TaskPriority.Medium,
            ProjectId = project.Id
        }, cancellationToken: TestContext.Current.CancellationToken))
            .Content.ReadFromJsonAsync<ApiResponse<TaskResponse>>(cancellationToken: TestContext.Current.CancellationToken);

        var deleteResponse = await client.DeleteAsync($"/api/tasks/{created!.Data!.Id}", TestContext.Current.CancellationToken);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var tasks = await (await client.GetAsync($"/api/tasks/project/{project.Id}", TestContext.Current.CancellationToken))
            .Content.ReadFromJsonAsync<ApiResponse<List<TaskResponse>>>(cancellationToken: TestContext.Current.CancellationToken);

        tasks!.Data.Should().NotContain(t => t.Id == created.Data.Id);
    }

    [Fact]
    public async Task Should_return_404_when_getting_tasks_for_another_users_project()
    {
        var (_, client1) = await RegisteredClientAsync();
        var (_, client2) = await RegisteredClientAsync();

        var project = await CreateProjectAsync(client1);

        var response = await client2.GetAsync($"/api/tasks/project/{project.Id}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_return_400_when_creating_task_with_past_due_date()
    {
        var (_, client) = await RegisteredClientAsync();
        var project = await CreateProjectAsync(client);

        var response = await client.PostAsJsonAsync("/api/tasks", new
        {
            Title = "Bad Task",
            Description = "Past due",
            DueDate = DateTime.UtcNow.AddDays(-1),
            Priority = (int)TaskPriority.Medium,
            ProjectId = project.Id
        }, cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}