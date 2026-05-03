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

    // GET /api/tasks/{projectId} — toutes les tâches d'un projet
    [HttpGet("{projectId}")]
    [ProducesResponseType(typeof(void),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void),StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTasks(int projectId) {
      var userId = GetCurrentUserId();
      if (userId == null) return StatusCode(401, new ApiError("401","Il vous faut un token, quand même >_>"));

      if (!await _db.Projects.AnyAsync(p => p.ProjetId == projectId && p.UserId == userId))
        return StatusCode(404, new ApiError("404","Tâche non trouvée ou non accessible"));

      var tasks = await _db.Tasks
        .Where(t => t.ProjectId == projectId)
        .ToListAsync();
      return Ok(tasks);
    }

    // POST /api/tasks — créer une tâche
    [HttpPost]
    [ProducesResponseType(typeof(void),StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(void),StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(void),StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto) {
      var userId = GetCurrentUserId();
      if (userId == null) return StatusCode(403, new ApiError("403","Désolée, mais il vous faut un token valide pour faire ça ^ᴗ^ᵕ"));

      if (!await _db.Projects.AnyAsync(p => p.ProjetId == dto.ProjectId && p.UserId == userId))
        return StatusCode(404, new ApiError("404","Projet absent ou non accessible"));

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
    [ProducesResponseType(typeof(void),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void),StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTask(int taskId, [FromBody] UpdateTaskDto dto) {
      var userId = GetCurrentUserId();
      if (userId == null) return StatusCode(401, new ApiError("401","Désolée, mais vous n'avez pas le droit de faire ça :("));

      var task = await _db.Tasks.FirstOrDefaultAsync(t => t.TaskId == taskId);
      if (task == null) return StatusCode(404, new ApiError("404","Désolée, mais il manque quelque chose ici..."));

      // On vérifie que la personne a bien le droit de toucher au projet
      if (!await _db.Projects.AnyAsync(p => p.ProjetId == task.ProjectId && p.UserId == userId))
        return StatusCode(404, new ApiError("404","Il manque quelque chose dans l'équation ¬_¬*"));

      // On évite de remplacer des champs qui existent déjà dans la DB
      if (dto.Title != null)         task.Title = dto.Title;
      if (dto.Description != null)   task.Description = dto.Description;
      if (dto.DueDate != null)       task.DueDate = dto.DueDate.Value;
      if (dto.CurrentStatus != null) task.CurrentStatus = dto.CurrentStatus.Value;

      await _db.SaveChangesAsync();
      return Ok(task);
    }

    // DELETE /api/tasks/{taskId} — YEET (╯°□°）╯︵ ┻━┻
    [HttpDelete("{taskId}")]
    [ProducesResponseType(typeof(void),StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void),StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(int taskId) {
      var userId = GetCurrentUserId();
      if (userId == null) return StatusCode(401, new ApiError("401","Vous n'avez pas le droit de faire ça ¬‸¬*"));

      var task = await _db.Tasks.FirstOrDefaultAsync(t => t.TaskId == taskId);
      if (task == null) return StatusCode(404, new ApiError("404","Cette tâche n'existe pas, impossible de la supprimer !"));

      if (!await _db.Projects.AnyAsync(p => p.ProjetId == task.ProjectId && p.UserId == userId))
        return StatusCode(404, new ApiError("404","Il manque un truc ici..."));

      // El famosó YEET
      _db.Tasks.Remove(task);
      await _db.SaveChangesAsync();
      return NoContent(); // 204 quand la tâche a bien été supprimée
    }
  }

  public record CreateTaskDto(string Title, DateTime DueDate, int ProjectId, ICollection<string>? Description);
  public record UpdateTaskDto(string? Title, ICollection<string>? Description, DateTime? DueDate, UserTask.Status? CurrentStatus);
}

