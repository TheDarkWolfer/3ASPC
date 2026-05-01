// J'ai tellement l'habitude de bash que j'ai bien failli mettre une shebang à cette ligne ᵕ-ᴗ-
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskFlow.Data;
using TaskFlow.Models;

// Controlleur pour les utilisateur.ices
namespace TaskFlow.Controllers {

  [ApiController]
  [Route("api/users")]
  public class UsersController : ControllerBase {

    // Connection à la DB et récupération de la configuration
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public UsersController(AppDbContext db, IConfiguration config) {
      _db = db;
      _config = config;
    }

    // Création de nouvel utilisateur.ice
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto) {

      // Check pour vérifier qu'on a bien un mail unique
      if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
        return Conflict("Cet email est déjà utilisé.");

      var user = new User {
        Username = dto.Username,
        Email    = dto.Email,
        Password = BCrypt.Net.BCrypt.HashPassword(dto.Password), // C'est pas dans le cahier des charges, mais ![Article 5 paragraphe 1 alinéa F du RGPD](https://gdpr-info.eu/art-5-gdpr/)
        Role     = Role.User
      };

      _db.Users.Add(user);
      await _db.SaveChangesAsync();

      return CreatedAtAction(null, null, new { user.Id, user.Username, user.Email, user.Role });
    }

    // Page de logins - on récupère les données utilisateur.ice depuis la DB SQLite
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto) {

      var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

      // Utilisateur pas trouvé OU mauvais mot de passe
      if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
        return Unauthorized("Email ou mot de passe incorrect !");


      // Si tout se passe bien, cooki- token JWT, pardon •𐃷•
      var token = GenerateJwtToken(user);
      return Ok(new { token });
    }

    // Création du token
    // Ingrédients : 
    // 30g de cryptographie, 
    // >256ml de secret JWT,
    // Un algorithme de hashage
    // Une pincée de maths
    // Faire cuire à 60°c pendant 200ms
    private string GenerateJwtToken(User user) {
      var claims = new[] {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name,           user.Username),
        new Claim(ClaimTypes.Email,          user.Email),
        new Claim(ClaimTypes.Role,           user.Role.ToString())
      };

      // Récupération de la clef depuis la config (CHANGEZ LA CLEF PAR DÉFAUT PLEASE !!!)
      var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      // Par défaut on donne un token qui est valable pendant 8 heures : à affiner selon les besoins de TaskFlow, mais 8h ça devrait le faire
      var token = new JwtSecurityToken(
        claims:   claims,
        expires:  DateTime.UtcNow.AddHours(8),
        signingCredentials: creds
      );

      // On retourne le token bien propre
      return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }

  // Structure des requêtes (DTOs)
  // Pour des raisons de sécurité, on laisse pas les utilisateur.ices choisir leur rôle ;
  // Les admins sont créé.es manuellement, le reste des utilisateur.ices héritent du rôle 
  // d'User classique
  public record RegisterDto(string Username, string Email, string Password);
  public record LoginDto(string Email, string Password);
}
