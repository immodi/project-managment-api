using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ProjectRepository(ApplicationDbContext context) : IProjectRepository
{
    public async Task<Project?> GetByIdAsync(Guid id)
    {
        return await context.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Project>> GetByUserIdAsync(Guid userId)
    {
        return await context.Projects
            .Where(p => p.UserId == userId)
            .ToListAsync();
    }

    public async Task AddAsync(Project project)
    {
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Project project)
    {
        context.Projects.Update(project);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Project project)
    {
        context.Projects.Remove(project);
        await context.SaveChangesAsync();
    }
}