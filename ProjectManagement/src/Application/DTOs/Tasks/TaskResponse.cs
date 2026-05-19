using Domain.Enums;
using TaskStatus = Domain.Enums.TaskStatus;

namespace Application.DTOs.Tasks;


public class TaskResponse
{
    public Guid Id { get; set; }
    public required string Title { get; set; } 
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime DueDate { get; set; }
    public Guid ProjectId { get; set; }
}