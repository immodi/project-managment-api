using Application.DTOs.Projects;

namespace Application.Interfaces;


public interface IProjectService
{
    Task<ProjectResponse> CreateAsync(CreateProjectRequest request, Guid userId);
    Task<List<ProjectResponse>> GetAllAsync(Guid userId);
    Task<ProjectResponse> GetByIdAsync(Guid id, Guid userId);
    Task UpdateAsync(Guid id, UpdateProjectRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}