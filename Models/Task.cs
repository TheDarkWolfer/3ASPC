using System.ComponentModel.DataAnnotations;

// Création d'un modèle de données
namespace TaskFlow.Models {

  // Au cas où, pour éviter la confusion avec les définitions de tâche internes à C#
  // Probablement inutile, mais bon
  public class UserTask {

    public enum Status {
      To_Do,
      Active,
      Done
    }

    [Key] // Clef primaire - /!\ différente de l'id de tâche /!\
    public int TaskId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public Status CurrentStatus { get; set; } = Status.To_Do;

    // SVP on préfère les dates de rendu de plus de deux mois ᵕ-ᴗ-
    // Aussi, pas de block [Required] pour celle-ci
    public DateTime DueDate { get; set; }

    // Clef étrangère (externe ?) pour l'ID de projet lié
    [Required]
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    // Description de la tâche
    public ICollection<UserTask> Tasks { get; set; } = new List<UserTask>(); 

  }
} 
