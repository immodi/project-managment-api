namespace Application.DTOs.Projects;


public class UpdateProjectRequest
{
    public required string Name { get; set; } 
    public string? Description { get; set; }
}