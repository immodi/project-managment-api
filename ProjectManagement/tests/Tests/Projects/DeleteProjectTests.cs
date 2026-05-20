using Application.Common.Exceptions;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Moq;
using Xunit;

namespace Tests.Projects;

public class DeleteProjectTests
{
    private readonly Mock<IProjectRepository> _projects = new();
    private readonly ProjectService _service;

    public DeleteProjectTests()
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
            Description = "Desc",
            UserId = userId,
            User = new User
            {
                Id = userId,
                Email = "test@test.com",
                PasswordHash = "hash"
            }
        };

    [Fact]
    public async Task Should_delete_project_when_user_is_owner()
    {
        var userId = UserId();
        var project = CreateProject(userId);

        _projects
            .Setup(x => x.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        await _service.DeleteAsync(project.Id, userId);

        _projects.Verify(x => x.DeleteAsync(project), Times.Once);
    }

    [Fact]
    public async Task Should_throw_when_project_not_found()
    {
        var projectId = ProjectId();

        _projects
            .Setup(x => x.GetByIdAsync(projectId))
            .ReturnsAsync((Project?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.DeleteAsync(projectId, UserId()));
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
            _service.DeleteAsync(project.Id, otherUserId));
    }
}