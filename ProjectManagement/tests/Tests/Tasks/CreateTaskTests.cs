using Application.Common.Exceptions;
using Application.DTOs.Tasks;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.Tasks;

public class CreateTaskTests
{
    private readonly Mock<ITaskRepository> _tasks = new();
    private readonly Mock<IProjectRepository> _projects = new();

    private readonly TaskService _service;

    public CreateTaskTests()
    {
        _service = new TaskService(_tasks.Object, _projects.Object);
    }

    private static Guid UserId() => Guid.NewGuid();

    [Fact]
    public async Task Should_create_task_when_project_belongs_to_user()
    {
        var userId = UserId();

        var project = new Project
        {
            Id = Guid.NewGuid(),
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

        var request = new CreateTaskRequest
        {
            Title = "New Task",
            Description = "Task Desc",
            DueDate = DateTime.UtcNow.AddDays(2),
            Priority = TaskPriority.High,
            ProjectId = project.Id
        };

        _projects.Setup(x => x.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _tasks.Setup(x => x.AddAsync(It.IsAny<TaskItem>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(request, userId);

        result.Title.Should().Be(request.Title);
        result.ProjectId.Should().Be(project.Id);

        _tasks.Verify(x => x.AddAsync(It.Is<TaskItem>(t =>
            t.Title == request.Title &&
            t.ProjectId == project.Id &&
            t.Priority == TaskPriority.High
        )), Times.Once);
    }

    [Fact]
    public async Task Should_throw_when_project_not_found()
    {
        var request = new CreateTaskRequest
        {
            Title = "Task",
            DueDate = DateTime.UtcNow.AddDays(2),
            Priority = TaskPriority.Medium,
            ProjectId = Guid.NewGuid()
        };

        _projects.Setup(x => x.GetByIdAsync(request.ProjectId))
            .ReturnsAsync((Project?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CreateAsync(request, UserId()));
    }

    [Fact]
    public async Task Should_throw_when_project_does_not_belong_to_user()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Project",
            Description = "Desc",
            UserId = ownerId,
            User = new User
            {
                Id = ownerId,
                Email = "owner@test.com",
                PasswordHash = "hash"
            }
        };

        var request = new CreateTaskRequest
        {
            Title = "Task",
            DueDate = DateTime.UtcNow.AddDays(2),
            Priority = TaskPriority.Medium,
            ProjectId = project.Id
        };

        _projects.Setup(x => x.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CreateAsync(request, otherUserId));
    }
}