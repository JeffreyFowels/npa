using System.ComponentModel.DataAnnotations;

namespace NPA.Data.Models;

public class ProjectTask
{
    public int Id { get; set; }

    public int MilestoneId { get; set; }
    public Milestone? Milestone { get; set; }

    [Required, MaxLength(500)]
    public string TaskName { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string AdditionalDetail { get; set; } = string.Empty;

    public Priority Priority { get; set; } = Priority.Low;
    public TaskStatus Status { get; set; } = TaskStatus.NotStarted;
    public int PercentComplete { get; set; }

    public string? ConsultantId { get; set; }
    public ApplicationUser? Consultant { get; set; }

    [MaxLength(4000)]
    public string Comment { get; set; } = string.Empty;

    public DateTime? LastCommentDate { get; set; }

    [MaxLength(500)]
    public string Timeline { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Output { get; set; } = string.Empty;

    public DateTime? CompletionDate { get; set; }

    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
