using Application.Common.Exceptions;
using Application.DTOs.Projects;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class ProjectService(
    IProjectRepository projectRepository
) : IProjectService
{
    public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request, Guid userId)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            UserId = userId,
            User = null!
        };

        await projectRepository.AddAsync(project);

        return Map(project);
    }

    public async Task<List<ProjectResponse>> GetAllAsync(Guid userId)
    {
        var projects = await projectRepository.GetByUserIdAsync(userId);
        return projects.Select(Map).ToList();
    }

    public async Task<ProjectResponse> GetByIdAsync(Guid id, Guid userId)
    {
        var project = await projectRepository.GetByIdAsync(id);

        if (project is null || project.UserId != userId)
            throw new NotFoundException("Project not found");

        return Map(project);
    }

    public async Task UpdateAsync(Guid id, UpdateProjectRequest request, Guid userId)
    {
        var project = await projectRepository.GetByIdAsync(id);

        if (project is null || project.UserId != userId)
            throw new NotFoundException("Project not found");

        project.Name = request.Name;
        project.Description = request.Description;

        await projectRepository.UpdateAsync(project);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var project = await projectRepository.GetByIdAsync(id);

        if (project is null || project.UserId != userId)
            throw new NotFoundException("Project not found");

        await projectRepository.DeleteAsync(project);
    }

    private static ProjectResponse Map(Project p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        CreatedAt = p.CreatedAt
    };
}