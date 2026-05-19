using Application.Common.Exceptions;
using Application.DTOs.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using TaskStatus = Domain.Enums.TaskStatus;

namespace Application.Services;

public class TaskService(
    ITaskRepository taskRepository,
    IProjectRepository projectRepository
) : ITaskService
{
    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, Guid userId)
    {
        var project = await projectRepository.GetByIdAsync(request.ProjectId);

        if (project is null || project.UserId != userId)
            throw new NotFoundException("Project not found");

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            Priority = request.Priority,
            ProjectId = request.ProjectId,
            Project = project
        };

        await taskRepository.AddAsync(task);

        return Map(task);
    }

    public async Task UpdateAsync(Guid taskId, UpdateTaskRequest request, Guid userId)
    {
        var task = await taskRepository.GetByIdAsync(taskId);

        if (task is null || task.Project.UserId != userId)
            throw new NotFoundException("Task not found");

        task.Title = request.Title;
        task.Description = request.Description;
        task.DueDate = request.DueDate;
        task.Priority = request.Priority;

        await taskRepository.UpdateAsync(task);
    }

    public async Task UpdateStatusAsync(Guid taskId, TaskStatus status, Guid userId)
    {
        var task = await taskRepository.GetByIdAsync(taskId);

        if (task is null || task.Project.UserId != userId)
            throw new NotFoundException("Task not found");

        task.Status = status;

        await taskRepository.UpdateAsync(task);
    }

    public async Task<List<TaskResponse>> GetByProjectIdAsync(Guid projectId, Guid userId)
    {
        var project = await projectRepository.GetByIdAsync(projectId);

        if (project is null || project.UserId != userId)
            throw new NotFoundException("Project not found");

        var tasks = await taskRepository.GetByProjectIdAsync(projectId);
        return tasks.Select(Map).ToList();
    }

    public async Task DeleteAsync(Guid taskId, Guid userId)
    {
        var task = await taskRepository.GetByIdAsync(taskId);

        if (task is null || task.Project.UserId != userId)
            throw new NotFoundException("Task not found");

        await taskRepository.DeleteAsync(task);
    }

    private static TaskResponse Map(TaskItem t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Status = t.Status,
        Priority = t.Priority,
        DueDate = t.DueDate,
        ProjectId = t.ProjectId
    };
}