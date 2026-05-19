namespace Domain.Entities;

public class Project
{
    public Guid Id { get; set; }
    public required string Name { get; set; } 
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid UserId { get; set; }
    public required User User { get; set; }

    public List<TaskItem> Tasks { get; set; } = [];
}