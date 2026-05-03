using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskFlow.Data;
using TaskFlow.Models;

namespace TaskFlow.Controllers {

  [ApiController]
  [Route("api/projets")]
  [Authorize]
  public class ProjectsController : ControllerBase {
    private readonly AppDbContext _db;

    // Objet permettant l'accès à la DB
    public ProjectsController(AppDbContext db) {
      _db = db;
    }

    // Récupération des différents projets auxquels l'utilisateur.ice a accès'
    // GET /api/projets
    [HttpGet]
    [ProducesResponseType(typeof(void),StatusCodes.Status200OK)] // Pour les utilisateur.ices autentifiés
    [ProducesResponseType(typeof(void),StatusCodes.Status401Unauthorized)] // Pour le reste >:(
    public async Task<IActionResult> GetProjects() {
      var userId = GetCurrentUserId();

      if (userId == null)
        return Unauthorized(new ApiError("401", "Token invalide ou utilisateur introuvable."));

      // On utilise la syntaxe d'EntityFramework au lieu de faire 
      // une requête SQL directement
      // Ça a l'avantage de rendre tout ça un peu plus résistant aux 
      // injections SQL ╮.❛ᴗ❛.╭
      var projets = await _db.Projects
        .Where(p => p.UserId == userId)
        .Select(p => new ProjectResponseDto(
          p.ProjetId,
          p.Name,
          p.Description,
          p.CreationDate,
          p.UserId
        ))
        .ToListAsync();

      return Ok(projets);
    }

    // GET /api/projets/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(void),StatusCodes.Status200OK)] // 200 quand tout se passe bien
    [ProducesResponseType(typeof(void),StatusCodes.Status400BadRequest)] // 400 quand on reçoit des mauvaises data
    [ProducesResponseType(typeof(void),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)] // 404 quand projet non existant 
    public async Task<IActionResult> GetProjectById(int id) {
      var userId = GetCurrentUserId();
      
      if (id <= 0) // J'espère que ça marche bien ça, j'ai trouvé ça dans les tréfons de la doc TwT
	return BadRequest(new ApiError("400","Requête malformée :("));

      if (userId == null)
        return Unauthorized(new ApiError("403", "Token invalide ou utilisateur introuvable <_<"));
      Console.WriteLine($"USER ID: {userId}");

      // Encore une fois, requête EntityFramework au lieu de sortir le SQL
      var project = await _db.Projects
        .Where(p => p.ProjetId == id && p.UserId == userId)
        .Select(p => new ProjectResponseDto(
          p.ProjetId,
          p.Name,
          p.Description,
          p.CreationDate,
          p.UserId
        ))
        .FirstOrDefaultAsync();

      if (project == null)
        return NotFound(new ApiError("404", "Projet introuvable ;_;"));
      return Ok(project);
    }

    // Création d'un nouveau projet (faut un token valide btw)
    // POST /api/projets
    [HttpPost]
    [ProducesResponseType(typeof(void),StatusCodes.Status201Created)] // Projet créé
    [ProducesResponseType(typeof(void),StatusCodes.Status400BadRequest)] // Si on reçoit un body mal fait par l'utilisateur.ice
    [ProducesResponseType(typeof(void),StatusCodes.Status401Unauthorized)] // Pas de token -> La PORTE !
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto) {
      var userId = GetCurrentUserId();

      if (userId == null)
        return Unauthorized(new ApiError("403", "Token invalide ou utilisateur introuvable."));

      if (string.IsNullOrWhiteSpace(dto.Name))
        return BadRequest(new ApiError("400", "Le nom du projet est obligatoire."));

      var project = new Project {
        Name = dto.Name.Trim(),
        Description = dto.Description?.Trim(),
        CreationDate = DateTime.UtcNow,
        UserId = userId.Value
      };

      _db.Projects.Add(project);
      await _db.SaveChangesAsync();

      var response = new ProjectResponseDto(
        project.ProjetId,
        project.Name,
        project.Description,
        project.CreationDate,
        project.UserId
      );
      return CreatedAtAction(nameof(GetProjectById), new { id = project.ProjetId }, response);
    }

    // Modification d'un projet qui existe déjà
    // PUT /api/projets/{id}
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(void),StatusCodes.Status200OK)] // PUT qui a marché
    [ProducesResponseType(typeof(void),StatusCodes.Status400BadRequest)] // Requête malformée
    [ProducesResponseType(typeof(void),StatusCodes.Status403Forbidden)] // Utilisateur.ice qui veut changer quelque chose sur le projet de quelqu'un d'autre
    [ProducesResponseType(typeof(void),StatusCodes.Status404NotFound)] // El classico 404
    public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectDto dto) {
      var userId = GetCurrentUserId();

      if (userId == null)
        return Unauthorized(new ApiError("403", "Token invalide ou utilisateur introuvable."));

      var project = await _db.Projects.FirstOrDefaultAsync(p => p.ProjetId == id && p.UserId == userId);
      /*                                                                ▲                  ▲
       *                                                                │                  │
       * J'en profite pour expliquer un p'tit truc                Là c'est juste choper    │
       * sur cette requête EF                                     le bon projet            │
       *                                                                                   │
       *                                                            On s'assure que le projet a bien comme
       *                                                            propriétaire l'utilisateur.ice concerné.e
       */

      if (project == null)
        return NotFound(new ApiError("404", "Projet introuvable."));

      if (string.IsNullOrWhiteSpace(dto.Name))
        return BadRequest(new ApiError("400", "Le nom du projet est obligatoire."));

      // Nom de projet obligatoire, mais description optionnelle
      project.Name = dto.Name.Trim();
      project.Description = dto.Description?.Trim();

      await _db.SaveChangesAsync();

      // Renvoi d'un OK et des infos que l'utilisateur.ice a donné ; 
      // Ça permet d'avoir un peu de retour, on pourrait hypothétiquement
      // faire un site avec React qui serait bien responsive & tout
      return Ok(new ProjectResponseDto(
        project.ProjetId,
        project.Name,
        project.Description,
        project.CreationDate,
        project.UserId
      ));
    }

    // 
    // DELETE /api/projets/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(void),StatusCodes.Status202Accepted)] // "Accepted" pour la suppression ╮.❛ᴗ❛.╭
    [ProducesResponseType(typeof(void),StatusCodes.Status403Forbidden)] 
    [ProducesResponseType(typeof(void),StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProject(int id) {
      var userId = GetCurrentUserId();

      if (userId == null)
        return Unauthorized(new ApiError("403", "Token invalide ou utilisateur introuvable."));

      var project = await _db.Projects.FirstOrDefaultAsync(p => p.ProjetId == id && p.UserId == userId);

      if (project == null)
        return NotFound(new ApiError("404", "Projet introuvable ou suppression non autorisée."));

      _db.Projects.Remove(project);
      await _db.SaveChangesAsync();

      // Projet bien supprimé -> ദ്ദിᵔᗜᵔ
      return Ok(new {
        code = "202",
        message = "Projet supprimé avec succès."
      });
    }

    private int? GetCurrentUserId() {
      var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (int.TryParse(userIdClaim, out var userId))
        return userId;
      return null;
    }
  }

  // DTOs pour les différentes actions du CRUD
  public record CreateProjectDto(string Name,string? Description);
  public record UpdateProjectDto(string Name,string? Description);
  public record ProjectResponseDto(int Id,string Name,string? Description,DateTime CreationDate,int UserId);
  public record ApiError(string Code,string Message);
}
