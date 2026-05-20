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

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _tasks = new();
    private readonly Mock<IProjectRepository> _projects = new();

    private readonly TaskService _service;

    public TaskServiceTests()
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

    [Fact]
    public async Task Create_Should_add_task_when_project_belongs_to_user()
    {
        var userId = UserId();
        var project = CreateProject(userId);

        var request = new CreateTaskRequest
        {
            Title = "Task 1",
            Description = "Desc",
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

        _tasks.Verify(x => x.AddAsync(It.Is<TaskItem>(t =>
            t.Title == request.Title &&
            t.ProjectId == project.Id
        )), Times.Once);
    }

    [Fact]
    public async Task Create_Should_throw_when_project_not_found()
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
            _service.CreateAsync(request, Guid.NewGuid()));
    }

    [Fact]
    public async Task Update_Should_modify_task_when_user_is_owner()
    {
        var userId = UserId();
        var project = CreateProject(userId);

        var task = new TaskItem
        {
            Id = TaskId(),
            Title = "Old",
            Description = "Old",
            DueDate = DateTime.UtcNow.AddDays(1),
            Priority = TaskPriority.Low,
            ProjectId = project.Id,
            Project = project
        };

        _tasks.Setup(x => x.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        var request = new UpdateTaskRequest
        {
            Title = "New",
            Description = "Updated",
            DueDate = DateTime.UtcNow.AddDays(3),
            Priority = TaskPriority.High
        };

        await _service.UpdateAsync(task.Id, request, userId);

        task.Title.Should().Be("New");
        task.Priority.Should().Be(TaskPriority.High);

        _tasks.Verify(x => x.UpdateAsync(task), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_Should_change_status()
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

        await _service.UpdateStatusAsync(task.Id, TaskStatus.Done, userId);

        task.Status.Should().Be(TaskStatus.Done);

        _tasks.Verify(x => x.UpdateAsync(task), Times.Once);
    }

    [Fact]
    public async Task Delete_Should_remove_task()
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
            ProjectId = project.Id,
            Project = project
        };

        _tasks.Setup(x => x.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        await _service.DeleteAsync(task.Id, userId);

        _tasks.Verify(x => x.DeleteAsync(task), Times.Once);
    }

    [Fact]
    public async Task GetByProject_Should_return_tasks_when_owner_matches()
    {
        var userId = UserId();
        var project = CreateProject(userId);

        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Id = TaskId(),
                Title = "T1",
                Description = "D1",
                DueDate = DateTime.UtcNow.AddDays(1),
                Priority = TaskPriority.Low,
                ProjectId = project.Id,
                Project = project
            }
        };

        _projects.Setup(x => x.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _tasks.Setup(x => x.GetByProjectIdAsync(project.Id))
            .ReturnsAsync(tasks);

        var result = await _service.GetByProjectIdAsync(project.Id, userId);

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("T1");
    }
}