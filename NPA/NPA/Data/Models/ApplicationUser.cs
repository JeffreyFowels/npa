using Microsoft.AspNetCore.Identity;

namespace NPA.Data.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public AppRole Role { get; set; } = AppRole.StandardUser;
    public bool IsEntraUser { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    public ICollection<ProjectAssignment> ProjectAssignments { get; set; } = [];
}
