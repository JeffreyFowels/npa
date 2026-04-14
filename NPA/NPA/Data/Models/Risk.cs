using System.ComponentModel.DataAnnotations;

namespace NPA.Data.Models;

public class Risk
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public RiskStatus Status { get; set; } = RiskStatus.Identified;
    public RiskLikelihood Likelihood { get; set; } = RiskLikelihood.Possible;
    public RiskImpact Impact { get; set; } = RiskImpact.Moderate;

    [MaxLength(2000)]
    public string MitigationPlan { get; set; } = string.Empty;

    public string? OwnerId { get; set; }
    public ApplicationUser? Owner { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>Calculated risk score: Likelihood × Impact (1–25).</summary>
    public int RiskScore => (int)Likelihood * (int)Impact;
}
