using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskFlow.Data;
using TaskFlow.Models;

namespace TaskFlow.Controllers {

  [ApiController]
  [Route("api/tasks")]
  [Authorize] // <- Ce petit truc restreint l'accès aux porteur.euses de jeton JWT valide. J'aime bien ce p'tit truc, moi. ˶˃ᵕ˂˶
  public class TaskController : ControllerBase {

    private readonly AppDbContext _db; // Pas se marcher sur les pieds dans le code •⤙•

    public TaskController(AppDbContext db) {
      _db = db;
    }

    // GET /api/tasks/{projectId} — toutes les tâches d'un projet
    [HttpGet("{projectId}")]
    public async Task<IActionResult> GetTasks(int projectId) {
      var tasks = await _db.Tasks
        .Where(t => t.ProjectId == projectId)
        .ToListAsync();
      return Ok(tasks);
    }

    // POST /api/tasks — crée une nouvelle tâche
    // On y récupère toutes les données (si elles sont présentes) depuis la requête,
    // titre, status, date à rendre (optionnel), ID du projet rattaché, et description
    // (please utilisez la description, elle a été difficile à implémenter >﹏<)
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto) {
      var task = new UserTask {
	Title         = dto.Title,
	CurrentStatus = UserTask.Status.To_Do,
	DueDate       = dto.DueDate,
	ProjectId     = dto.ProjectId,
	Description   = dto.Description ?? new List<string>() // ALORS FUN FACT FAUT PAS L'OUBLIER CELLE-LÀ
      };


      // Enregistrement de la modif
      _db.Tasks.Add(task);
      await _db.SaveChangesAsync();

      // Hello, vous avez demandé un changement ? Le voici !
      return CreatedAtAction(null, null, task);
    }
  }

  // Ajout d'un DTO pour harmoniser le tout
  public record CreateTaskDto(string Title, DateTime DueDate, int ProjectId, ICollection<string>? Description);

}

