using Application.DTOs.Tasks;
using TaskStatus = Domain.Enums.TaskStatus;

namespace Application.Interfaces;

public interface ITaskService
{
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, Guid userId);

    Task UpdateAsync(Guid taskId, UpdateTaskRequest request, Guid userId);

    Task UpdateStatusAsync(Guid taskId, TaskStatus status, Guid userId);

    Task<List<TaskResponse>> GetByProjectIdAsync(Guid projectId, Guid userId);

    Task DeleteAsync(Guid taskId, Guid userId);
}