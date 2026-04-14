namespace NPA.Data.Models;

public class ProjectAssignment
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string AssignedById { get; set; } = string.Empty;
}
