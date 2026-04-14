using System.ComponentModel.DataAnnotations;

namespace NPA.Data.Models;

public class Milestone
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }

    public ICollection<ProjectTask> Tasks { get; set; } = [];

    /// <summary>Average percent complete across all tasks.</summary>
    public double AverageProgress => Tasks.Count == 0 ? 0 : Tasks.Average(t => t.PercentComplete);
}
