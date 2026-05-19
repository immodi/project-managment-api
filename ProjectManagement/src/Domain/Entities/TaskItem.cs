using Domain.Enums;
using TaskStatus = Domain.Enums.TaskStatus;

namespace Domain.Entities;

public class TaskItem
{
    public Guid Id { get; set; }

    public required string Title { get; set; } 
    public string? Description { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime DueDate { get; set; }

    public Guid ProjectId { get; set; }
    public required Project Project { get; set; } 
}