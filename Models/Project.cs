using System.ComponentModel.DataAnnotations;

// Création d'un modèle de données
namespace TaskFlow.Models {

  public class Project {
    [Key] // Clef primaire - /!\ différente de l'id de propriétaire /!\
    public int ProjectId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    // Description optionelle (utilisez-la, c'est dans le code ˶˃ᵕ˂˶)
    public string? Description { get; set;}

    [Required]
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    // Clef étrangère (externe ?) pour l'ID d'utilisateur.ice propriétaire.
    [Required]
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    // Liste de tâches - même renommage pour éviter la confusion que Task.cs:17
    public ICollection<UserTask> Tasks { get; set; } = new List<UserTask>();
  }
}
