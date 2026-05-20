using Application.Common.Exceptions;
using Application.DTOs.Projects;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.Projects;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _projects = new();

    private readonly ProjectService _service;

    public ProjectServiceTests()
    {
        _service = new ProjectService(_projects.Object);
    }

    private static Guid UserId() => Guid.NewGuid();
    private static Guid ProjectId() => Guid.NewGuid();

    [Fact]
    public async Task Create_Should_return_project_response()
    {
        var userId = UserId();

        var request = new CreateProjectRequest
        {
            Name = "Test Project",
            Description = "Desc"
        };

        _projects.Setup(x => x.AddAsync(It.IsAny<Project>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(request, userId);

        result.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);

        _projects.Verify(x => x.AddAsync(It.Is<Project>(p =>
            p.Name == request.Name &&
            p.Description == request.Description &&
            p.UserId == userId
        )), Times.Once);
    }

    [Fact]
    public async Task GetAll_Should_return_user_projects()
    {
        var userId = UserId();

        var projects = new List<Project>
        {
            new Project
            {
                Id = ProjectId(),
                Name = "P1",
                Description = "D1",
                UserId = userId,
                User = new User
                {
                    Id = userId,
                    Email = "test@test.com",
                    PasswordHash = "hash"
                }
            }
        };

        _projects.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(projects);

        var result = await _service.GetAllAsync(userId);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("P1");
    }

    [Fact]
    public async Task GetById_Should_return_project_when_owner_matches()
    {
        var userId = UserId();
        var projectId = ProjectId();

        var project = new Project
        {
            Id = projectId,
            Name = "P1",
            Description = "D1",
            UserId = userId,
            User = new User
            {
                Id = userId,
                Email = "test@test.com",
                PasswordHash = "hash"
            }
        };

        _projects.Setup(x => x.GetByIdAsync(projectId))
            .ReturnsAsync(project);

        var result = await _service.GetByIdAsync(projectId, userId);

        result.Id.Should().Be(projectId);
    }

    [Fact]
    public async Task GetById_Should_throw_when_not_found_or_not_owner()
    {
        var userId = UserId();
        var projectId = ProjectId();

        _projects.Setup(x => x.GetByIdAsync(projectId))
            .ReturnsAsync((Project?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetByIdAsync(projectId, userId));
    }

    [Fact]
    public async Task Update_Should_modify_project()
    {
        var userId = UserId();
        var projectId = ProjectId();

        var project = new Project
        {
            Id = projectId,
            Name = "Old",
            Description = "Old",
            UserId = userId,
            User = new User
            {
                Id = userId,
                Email = "test@test.com",
                PasswordHash = "hash"
            }
        };

        _projects.Setup(x => x.GetByIdAsync(projectId))
            .ReturnsAsync(project);

        var request = new UpdateProjectRequest
        {
            Name = "New",
            Description = "New Desc"
        };

        await _service.UpdateAsync(projectId, request, userId);

        project.Name.Should().Be("New");
        project.Description.Should().Be("New Desc");

        _projects.Verify(x => x.UpdateAsync(project), Times.Once);
    }

    [Fact]
    public async Task Delete_Should_remove_project()
    {
        var userId = UserId();
        var projectId = ProjectId();

        var project = new Project
        {
            Id = projectId,
            Name = "ToDelete",
            Description = "Desc",
            UserId = userId,
            User = new User
            {
                Id = userId,
                Email = "test@test.com",
                PasswordHash = "hash"
            }
        };

        _projects.Setup(x => x.GetByIdAsync(projectId))
            .ReturnsAsync(project);

        await _service.DeleteAsync(projectId, userId);

        _projects.Verify(x => x.DeleteAsync(project), Times.Once);
    }
}