using Domain.Enums;

namespace Application.DTOs.Tasks;


public class CreateTaskRequest
{
    public required string Title { get; set; } 
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public TaskPriority Priority { get; set; }
    public Guid ProjectId { get; set; }
}