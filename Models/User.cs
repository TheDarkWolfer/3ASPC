using System.ComponentModel.DataAnnotation;

// Création d'un modèle de données
namespace TaskFlow.Models {
  public enum Role
  {
    Admin,
    User,
  }

  public class User {
    public int Id { get; set; }

    [Required]
    public string Username { get; set;} = string.Empty;

    [Required]
    [EmailAddress]
    public string EmailAddress { get ; set; } = string.Empty;

    // Important : utiliser une fonction de hashage : RGPD les enfants !!!
    [Required]
    public string Password = { get; set; } = string.Empty;
  }
}
