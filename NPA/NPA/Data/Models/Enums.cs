namespace NPA.Data.Models;

public enum ProjectStatus
{
    NotStarted,
    InProgress,
    OnHold,
    Completed,
    Cancelled
}

public enum Priority
{
    Low,
    Medium,
    High,
    Critical
}

public enum RiskStatus
{
    Identified,
    Assessing,
    Mitigating,
    Resolved,
    Accepted
}

public enum RiskLikelihood
{
    Rare,
    Unlikely,
    Possible,
    Likely,
    AlmostCertain
}

public enum RiskImpact
{
    Negligible,
    Minor,
    Moderate,
    Major,
    Severe
}

public enum IssueStatus
{
    Open,
    InProgress,
    Resolved,
    Closed
}

public enum IssueSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum AppRole
{
    Admin,
    ProjectManager,
    StandardUser
}
