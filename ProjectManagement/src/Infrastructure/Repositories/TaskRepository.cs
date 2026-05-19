using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TaskRepository(ApplicationDbContext context) : ITaskRepository
{
    public async Task<TaskItem?> GetByIdAsync(Guid id)
    {
        return await context.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<TaskItem>> GetByProjectIdAsync(Guid projectId)
    {
        return await context.Tasks
            .Where(t => t.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task AddAsync(TaskItem task)
    {
        await context.Tasks.AddAsync(task);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TaskItem task)
    {
        context.Tasks.Update(task);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TaskItem task)
    {
        context.Tasks.Remove(task);
        await context.SaveChangesAsync();
    }
}