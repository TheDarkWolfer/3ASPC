using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;

// Je sais pas si c'est mon LSP qui est mal configuré, mais le C# 
// ressemble juste à... Une grosse config ? Et pas du code ?
namespace TaskFlow.Data {
  public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<UserTask> Tasks { get; set; }
  }
}
