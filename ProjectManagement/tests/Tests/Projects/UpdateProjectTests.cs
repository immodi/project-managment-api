using Application.Common.Exceptions;
using Application.DTOs.Projects;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.Projects;

public class UpdateProjectTests
{
    private readonly Mock<IProjectRepository> _projects = new();
    private readonly ProjectService _service;

    public UpdateProjectTests()
    {
        _service = new ProjectService(_projects.Object);
    }

    private static Guid UserId() => Guid.NewGuid();
    private static Guid ProjectId() => Guid.NewGuid();

    private static Project CreateProject(Guid userId)
        => new()
        {
            Id = ProjectId(),
            Name = "Old Name",
            Description = "Old Desc",
            UserId = userId,
            User = new User
            {
                Id = userId,
                Email = "test@test.com",
                PasswordHash = "hash"
            }
        };

    [Fact]
    public async Task Should_update_project_when_user_is_owner()
    {
        var userId = UserId();
        var project = CreateProject(userId);

        _projects
            .Setup(x => x.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        var request = new UpdateProjectRequest
        {
            Name = "New Name",
            Description = "New Desc"
        };

        await _service.UpdateAsync(project.Id, request, userId);

        project.Name.Should().Be("New Name");
        project.Description.Should().Be("New Desc");

        _projects.Verify(x => x.UpdateAsync(project), Times.Once);
    }

    [Fact]
    public async Task Should_throw_when_project_not_found()
    {
        var projectId = ProjectId();

        _projects
            .Setup(x => x.GetByIdAsync(projectId))
            .ReturnsAsync((Project?)null);

        var request = new UpdateProjectRequest
        {
            Name = "New",
            Description = "New"
        };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateAsync(projectId, request, UserId()));
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

        var request = new UpdateProjectRequest
        {
            Name = "New",
            Description = "New"
        };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateAsync(project.Id, request, otherUserId));
    }

    [Fact]
    public async Task Should_only_update_fields_provided()
    {
        var userId = UserId();
        var project = CreateProject(userId);

        _projects
            .Setup(x => x.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        var request = new UpdateProjectRequest
        {
            Name = "Partially Updated",
            Description = project.Description
        };

        await _service.UpdateAsync(project.Id, request, userId);

        project.Name.Should().Be("Partially Updated");
        project.Description.Should().Be("Old Desc");

        _projects.Verify(x => x.UpdateAsync(project), Times.Once);
    }
}