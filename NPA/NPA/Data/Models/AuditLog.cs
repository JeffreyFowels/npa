using System.ComponentModel.DataAnnotations;

namespace NPA.Data.Models;

public class AuditLog
{
    public long Id { get; set; }

    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    public string EntityId { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Action { get; set; } = string.Empty; // Created, Updated, Deleted, Restored

    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    public string UserId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string UserName { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
