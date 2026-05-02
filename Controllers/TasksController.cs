using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskFlow.Data;
using TaskFlow.Models;

namespace TaskFlow.Controllers {

  [ApiController]
  [Route("api/tasks")]
  [Authorize]
  public class TaskController : ControllerBase {

    private readonly AppDbContext _db;

    public TaskController(AppDbContext db) {
      _db = db;
    }

    private int? GetCurrentUserId() {
      var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
      return int.TryParse(claim, out var id) ? id : null;
    }

    // Vérifie que le projet appartient bien à l'user
    private async Task<bool> UserOwnsProject(int projectId, int userId) {
      return await _db.Projects.AnyAsync(p => p.ProjetId == projectId && p.UserId == userId);
    }

    // GET /api/tasks/{projectId} qui affiche toutes les tâches d'un projet
    [HttpGet("{projectId}")]
    public async Task<IActionResult> GetTasks(int projectId) {
      var userId = GetCurrentUserId();
      if (userId == null) return Unauthorized();

      if (!await UserOwnsProject(projectId, userId.Value))
        return Forbid();

      var tasks = await _db.Tasks
        .Where(t => t.ProjectId == projectId)
        .ToListAsync();
      return Ok(tasks);
    }

    // POST /api/tasks va créer une nouvelle tâche
    // (please utilisez la description, elle a été difficile à implémenter >﹏<)
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto) {
      var userId = GetCurrentUserId();
      if (userId == null) return Unauthorized();

      // On vérifie que le projet cible appartient bien à l'user
      if (!await UserOwnsProject(dto.ProjectId, userId.Value))
        return Forbid();

      var task = new UserTask {
        Title         = dto.Title,
        CurrentStatus = UserTask.Status.To_Do,
        DueDate       = dto.DueDate,
        ProjectId     = dto.ProjectId,
        Description   = dto.Description ?? new List<string>()
      };

      _db.Tasks.Add(task);
      await _db.SaveChangesAsync();

      return CreatedAtAction(null, null, task);
    }

    // PUT /api/tasks/{taskId} — modifie une tâche
    [HttpPut("{taskId}")]
    public async Task<IActionResult> UpdateTask(int taskId, [FromBody] UpdateTaskDto dto) {
      var userId = GetCurrentUserId();
      if (userId == null) return Unauthorized();

      // On join avec Project pour vérifier l'ownership en une requête
      var task = await _db.Tasks.FirstOrDefaultAsync(t => t.TaskId == taskId);
      if (task == null) return NotFound();
      if (!await UserOwnsProject(task.ProjectId, userId.Value)) return Forbid();

      if (dto.Title != null)         task.Title = dto.Title;
      if (dto.Description != null)   task.Description = dto.Description;
      if (dto.DueDate != null)       task.DueDate = dto.DueDate.Value;
      if (dto.CurrentStatus != null) task.CurrentStatus = dto.CurrentStatus.Value;

      await _db.SaveChangesAsync();
      return Ok(task);
    }

    
    // DELETE /api/tasks/{taskId} — YEET (╯°□°）╯︵ ┻━┻
    [HttpDelete("{taskId}")]
    public async Task<IActionResult> DeleteTask(int taskId) {
      var userId = GetCurrentUserId();
      if (userId == null) return Unauthorized();

      var task = await _db.Tasks.FirstOrDefaultAsync(t => t.TaskId == taskId);
      if (task == null) return NotFound();
      if (!await UserOwnsProject(task.ProjectId, userId.Value)) return Forbid();

      _db.Tasks.Remove(task);
      await _db.SaveChangesAsync();
      return NoContent();
    }
  }

  public record CreateTaskDto(string Title, DateTime DueDate, int ProjectId, ICollection<string>? Description);
  public record UpdateTaskDto(string? Title, ICollection<string>? Description, DateTime? DueDate, UserTask.Status? CurrentStatus);
}

