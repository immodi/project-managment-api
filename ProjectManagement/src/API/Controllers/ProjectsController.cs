using API.Extensions;
using Application.Common;
using Application.DTOs.Projects;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController(IProjectService projectService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProjectResponse>>> Create([FromBody] CreateProjectRequest request)
    {
        var userId = User.GetUserId();
        var result = await projectService.CreateAsync(request, userId);

        return Ok(ApiResponse<ProjectResponse>.Ok(result));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ProjectResponse>>>> GetAll()
    {
        var userId = User.GetUserId();
        var result = await projectService.GetAllAsync(userId);

        return Ok(ApiResponse<List<ProjectResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectResponse>>> GetById(Guid id)
    {
        var userId = User.GetUserId();
        var result = await projectService.GetByIdAsync(id, userId);

        return Ok(ApiResponse<ProjectResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectRequest request)
    {
        var userId = User.GetUserId();
        await projectService.UpdateAsync(id, request, userId);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.GetUserId();
        await projectService.DeleteAsync(id, userId);

        return NoContent();
    }
}