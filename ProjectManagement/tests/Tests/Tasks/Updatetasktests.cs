using Application.Common.Exceptions;
using Application.DTOs.Tasks;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;
using TaskStatus = Domain.Enums.TaskStatus;

namespace Tests.Tasks;

public class UpdateTaskTests
{
    private readonly Mock<ITaskRepository> _tasks = new();
    private readonly Mock<IProjectRepository> _projects = new();
    private readonly TaskService _service;

    public UpdateTaskTests()
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

    private static TaskItem CreateTask(Project project)
        => new()
        {
            Id = TaskId(),
            Title = "Old Title",
            Description = "Old Desc",
            DueDate = DateTime.UtcNow.AddDays(1),
            Priority = TaskPriority.Low,
            Status = TaskStatus.Todo,
            ProjectId = project.Id,
            Project = project
        };

    [Fact]
    public async Task Should_update_all_fields_when_user_is_owner()
    {
        var userId = UserId();
        var project = CreateProject(userId);
        var task = CreateTask(project);

        _tasks.Setup(x => x.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        var request = new UpdateTaskRequest
        {
            Title = "New Title",
            Description = "New Desc",
            DueDate = DateTime.UtcNow.AddDays(5),
            Priority = TaskPriority.High
        };

        await _service.UpdateAsync(task.Id, request, userId);

        task.Title.Should().Be("New Title");
        task.Description.Should().Be("New Desc");
        task.Priority.Should().Be(TaskPriority.High);
        task.DueDate.Should().BeCloseTo(request.DueDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Should_call_repository_once_on_update()
    {
        var userId = UserId();
        var project = CreateProject(userId);
        var task = CreateTask(project);

        _tasks.Setup(x => x.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        var request = new UpdateTaskRequest
        {
            Title = "Updated",
            Description = "Updated",
            DueDate = DateTime.UtcNow.AddDays(3),
            Priority = TaskPriority.Medium
        };

        await _service.UpdateAsync(task.Id, request, userId);

        _tasks.Verify(x => x.UpdateAsync(task), Times.Once);
    }

    [Fact]
    public async Task Should_throw_when_task_not_found()
    {
        var taskId = TaskId();

        _tasks.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync((TaskItem?)null);

        var request = new UpdateTaskRequest
        {
            Title = "Title",
            Description = "Desc",
            DueDate = DateTime.UtcNow.AddDays(3),
            Priority = TaskPriority.Medium
        };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateAsync(taskId, request, UserId()));
    }

    [Fact]
    public async Task Should_throw_when_user_is_not_owner()
    {
        var ownerId = UserId();
        var otherUserId = UserId();
        var project = CreateProject(ownerId);
        var task = CreateTask(project);

        _tasks.Setup(x => x.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        var request = new UpdateTaskRequest
        {
            Title = "Title",
            Description = "Desc",
            DueDate = DateTime.UtcNow.AddDays(3),
            Priority = TaskPriority.Medium
        };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateAsync(task.Id, request, otherUserId));
    }

    [Fact]
    public async Task Should_not_change_status_on_update()
    {
        var userId = UserId();
        var project = CreateProject(userId);
        var task = CreateTask(project);
        task.Status = TaskStatus.InProgress;

        _tasks.Setup(x => x.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        var request = new UpdateTaskRequest
        {
            Title = "Updated",
            Description = "Updated",
            DueDate = DateTime.UtcNow.AddDays(3),
            Priority = TaskPriority.High
        };

        await _service.UpdateAsync(task.Id, request, userId);

        // UpdateAsync should not touch Status — only UpdateStatusAsync does
        task.Status.Should().Be(TaskStatus.InProgress);
    }
}