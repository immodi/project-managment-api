using Application.Common.Exceptions;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Moq;
using Xunit;
using TaskStatus = Domain.Enums.TaskStatus;

namespace Tests.Tasks;

public class DeleteTaskTests
{
    private readonly Mock<ITaskRepository> _tasks = new();
    private readonly Mock<IProjectRepository> _projects = new();

    private readonly TaskService _service;

    public DeleteTaskTests()
    {
        _service = new TaskService(_tasks.Object, _projects.Object);
    }

    private static Guid UserId() => Guid.NewGuid();
    private static Guid TaskId() => Guid.NewGuid();
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
    public async Task Should_delete_task_when_user_is_owner()
    {
        var userId = UserId();
        var project = CreateProject(userId);

        var task = new TaskItem
        {
            Id = TaskId(),
            Title = "Task",
            Description = "Desc",
            DueDate = DateTime.UtcNow.AddDays(1),
            Priority = TaskPriority.Medium,
            Status = TaskStatus.Todo,
            ProjectId = project.Id,
            Project = project
        };

        _tasks.Setup(x => x.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        await _service.DeleteAsync(task.Id, userId);

        _tasks.Verify(x => x.DeleteAsync(task), Times.Once);
    }

    [Fact]
    public async Task Should_throw_when_task_not_found()
    {
        var taskId = TaskId();

        _tasks.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.DeleteAsync(taskId, UserId()));
    }

    [Fact]
    public async Task Should_throw_when_user_is_not_owner()
    {
        var ownerId = UserId();
        var otherUserId = UserId();

        var project = CreateProject(ownerId);

        var task = new TaskItem
        {
            Id = TaskId(),
            Title = "Task",
            Description = "Desc",
            DueDate = DateTime.UtcNow.AddDays(1),
            Priority = TaskPriority.Medium,
            Status = TaskStatus.Todo,
            ProjectId = project.Id,
            Project = project
        };

        _tasks.Setup(x => x.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.DeleteAsync(task.Id, otherUserId));
    }
}