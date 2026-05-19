using TaskStatus = Domain.Enums.TaskStatus;

namespace Application.DTOs.Tasks;

public class UpdateTaskStatusRequest
{
    public TaskStatus Status { get; set; }
}