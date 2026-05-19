using Domain.Entities;

namespace Domain.Interfaces;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id);

    Task<List<TaskItem>> GetByProjectIdAsync(Guid projectId);

    Task AddAsync(TaskItem task);
    Task UpdateAsync(TaskItem task);
    Task DeleteAsync(TaskItem task);
}