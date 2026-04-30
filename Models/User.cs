using System.ComponentModel.DataAnnotations;

// Création d'un modèle de données
namespace TaskFlow.Models {
  // En fait ça c'est pour laisser un choix contrôlé sur les rôles
  public enum Role
  {
    Admin,
    User,
  }

  public class User {
    [Key] // Au cas où, on précise que c'est ça la clef primaire
    public int Id { get; set; }

    [Required]
    public Role Role { get; set; } = Role.User;

    [Required]
    public string Username { get; set;} = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    // Important : utiliser une fonction de hashage : RGPD les enfants !!!
    [Required]
    public string Password { get; set; } = string.Empty;
  }
}
