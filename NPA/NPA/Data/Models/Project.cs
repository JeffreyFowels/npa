using System.ComponentModel.DataAnnotations;

namespace NPA.Data.Models;

public class Project
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string PONumber { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Customer { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    public ProjectStatus Status { get; set; } = ProjectStatus.NotStarted;
    public Priority Priority { get; set; } = Priority.Medium;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public int PercentComplete { get; set; }

    public string CreatedById { get; set; } = string.Empty;
    public ApplicationUser? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<ProjectAssignment> Assignments { get; set; } = [];
    public ICollection<Risk> Risks { get; set; } = [];
    public ICollection<Issue> Issues { get; set; } = [];
    public ICollection<Milestone> Milestones { get; set; } = [];
}
