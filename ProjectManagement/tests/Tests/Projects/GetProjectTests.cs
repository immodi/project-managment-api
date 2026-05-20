using Application.Common.Exceptions;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.Projects;

public class GetProjectTests
{
    private readonly Mock<IProjectRepository> _projects = new();
    private readonly ProjectService _service;

    public GetProjectTests()
    {
        _service = new ProjectService(_projects.Object);
    }

    private static Guid UserId() => Guid.NewGuid();
    private static Guid ProjectId() => Guid.NewGuid();

    private static Project CreateProject(Guid userId)
        => new()
        {
            Id = ProjectId(),
            Name = "Project",
            Description = "Description",
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            User = new User
            {
                Id = userId,
                Email = "test@test.com",
                PasswordHash = "hash"
            }
        };

    [Fact]
    public async Task Should_return_project_when_user_is_owner()
    {
        var userId = UserId();
        var project = CreateProject(userId);

        _projects
            .Setup(x => x.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        var result = await _service.GetByIdAsync(project.Id, userId);

        result.Id.Should().Be(project.Id);
        result.Name.Should().Be(project.Name);
        result.Description.Should().Be(project.Description);
        result.CreatedAt.Should().Be(project.CreatedAt);
    }

    [Fact]
    public async Task Should_throw_when_project_not_found()
    {
        var projectId = ProjectId();

        _projects
            .Setup(x => x.GetByIdAsync(projectId))
            .ReturnsAsync((Project?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetByIdAsync(projectId, UserId()));
    }

    [Fact]
    public async Task Should_throw_when_user_is_not_owner()
    {
        var ownerId = UserId();
        var otherUserId = UserId();

        var project = CreateProject(ownerId);

        _projects
            .Setup(x => x.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetByIdAsync(project.Id, otherUserId));
    }
}