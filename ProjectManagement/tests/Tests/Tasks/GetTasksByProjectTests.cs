using Application.Common.Exceptions;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;
using TaskStatus = Domain.Enums.TaskStatus;

namespace Tests.Tasks;

public class GetTasksByProjectTests
{
    private readonly Mock<ITaskRepository> _tasks = new();
    private readonly Mock<IProjectRepository> _projects = new();
    private readonly TaskService _service;

    public GetTasksByProjectTests()
    {
        _service = new TaskService(_tasks.Object, _projects.Object);
    }

    private static Guid UserId() => Guid.NewGuid();
    private static Guid ProjectId() => Guid.NewGuid();
    private static Guid TaskId() => Guid.NewGuid();

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

    private static TaskItem CreateTask(Guid projectId)
        => new()
        {
            Id = TaskId(),
            Title = "Task 1",
            Description = "Desc",
            DueDate = DateTime.UtcNow.AddDays(1),
            Priority = TaskPriority.High,
            Status = TaskStatus.Todo,
            ProjectId = projectId,
            Project = CreateProject(projectId),
        };

    [Fact]
    public async Task Should_return_tasks_when_user_is_owner()
    {
        var userId = UserId();
        var project = CreateProject(userId);
        var task = CreateTask(project.Id);

        _projects
            .Setup(x => x.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _tasks
            .Setup(x => x.GetByProjectIdAsync(project.Id))
            .ReturnsAsync(new List<TaskItem> { task });

        var result = await _service.GetByProjectIdAsync(project.Id, userId);

        result.Should().HaveCount(1);
        result[0].Title.Should().Be(task.Title);
        result[0].ProjectId.Should().Be(project.Id);
    }

    [Fact]
    public async Task Should_throw_when_project_not_found()
    {
        var projectId = ProjectId();

        _projects
            .Setup(x => x.GetByIdAsync(projectId))
            .ReturnsAsync((Project?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetByProjectIdAsync(projectId, UserId()));
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
            _service.GetByProjectIdAsync(project.Id, otherUserId));
    }

    [Fact]
    public async Task Should_call_task_repository_once()
    {
        var userId = UserId();
        var project = CreateProject(userId);

        _projects
            .Setup(x => x.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _tasks
            .Setup(x => x.GetByProjectIdAsync(project.Id))
            .ReturnsAsync(new List<TaskItem>());

        await _service.GetByProjectIdAsync(project.Id, userId);

        _tasks.Verify(x => x.GetByProjectIdAsync(project.Id), Times.Once);
    }
}