using Application.DTOs.Projects;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.Projects;

public class CreateProjectTests
{
    private readonly Mock<IProjectRepository> _projects = new();
    private readonly ProjectService _service;

    public CreateProjectTests()
    {
        _service = new ProjectService(_projects.Object);
    }

    private static Guid UserId() => Guid.NewGuid();

    [Fact]
    public async Task Should_create_project_and_return_response()
    {
        var userId = UserId();

        var request = new CreateProjectRequest
        {
            Name = "My Project",
            Description = "Project Description"
        };

        _projects
            .Setup(x => x.AddAsync(It.IsAny<Project>()))
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
    public async Task Should_attach_user_id_to_created_project()
    {
        var userId = UserId();

        var request = new CreateProjectRequest
        {
            Name = "Another Project",
            Description = "Another Description"
        };

        Project? captured = null;

        _projects
            .Setup(x => x.AddAsync(It.IsAny<Project>()))
            .Callback<Project>(p => captured = p)
            .Returns(Task.CompletedTask);

        await _service.CreateAsync(request, userId);

        captured.Should().NotBeNull();
        captured!.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task Should_call_repository_once()
    {
        var userId = UserId();

        var request = new CreateProjectRequest
        {
            Name = "Proj",
            Description = "Desc"
        };

        _projects
            .Setup(x => x.AddAsync(It.IsAny<Project>()))
            .Returns(Task.CompletedTask);

        await _service.CreateAsync(request, userId);

        _projects.Verify(x => x.AddAsync(It.IsAny<Project>()), Times.Once);
    }
}