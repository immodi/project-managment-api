using API.Extensions;
using Application.Common;
using Application.DTOs.Tasks;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController(ITaskService taskService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TaskResponse>>> Create(
        [FromBody] CreateTaskRequest request)
    {
        var userId = User.GetUserId();
        var result = await taskService.CreateAsync(request, userId);

        return Ok(ApiResponse<TaskResponse>.Ok(result));
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<List<TaskResponse>>>> GetByProject(
        Guid projectId)
    {
        var userId = User.GetUserId();
        var result = await taskService.GetByProjectIdAsync(projectId, userId);

        return Ok(ApiResponse<List<TaskResponse>>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Update(
        Guid id,
        [FromBody] UpdateTaskRequest request)
    {
        var userId = User.GetUserId();

        await taskService.UpdateAsync(id, request, userId);

        return Ok(ApiResponse<string>.Ok("Task updated successfully"));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateStatus(
        Guid id,
        [FromBody] UpdateTaskStatusRequest request)
    {
        var userId = User.GetUserId();

        await taskService.UpdateStatusAsync(id, request.Status, userId);

        return Ok(ApiResponse<string>.Ok("Task status updated successfully"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id)
    {
        var userId = User.GetUserId();

        await taskService.DeleteAsync(id, userId);

        return Ok(ApiResponse<string>.Ok("Task deleted successfully"));
    }
}