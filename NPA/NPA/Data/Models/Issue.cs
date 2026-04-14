using System.ComponentModel.DataAnnotations;

namespace NPA.Data.Models;

public class Issue
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public IssueStatus Status { get; set; } = IssueStatus.Open;
    public IssueSeverity Severity { get; set; } = IssueSeverity.Medium;
    public Priority Priority { get; set; } = Priority.Medium;

    [MaxLength(2000)]
    public string Resolution { get; set; } = string.Empty;

    public string? AssignedToId { get; set; }
    public ApplicationUser? AssignedTo { get; set; }

    public string CreatedById { get; set; } = string.Empty;
    public ApplicationUser? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public bool IsDeleted { get; set; }
}
