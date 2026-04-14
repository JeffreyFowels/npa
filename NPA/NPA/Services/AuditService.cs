using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NPA.Data;
using NPA.Data.Models;

namespace NPA.Services;

public class AuditService(NpaDbContext db)
{
    public async Task LogAsync(string entityType, string entityId, string action, string userId, string userName, object? oldValues = null, object? newValues = null)
    {
        var log = new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            UserId = userId,
            UserName = userName,
            OldValues = oldValues is not null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues is not null ? JsonSerializer.Serialize(newValues) : null,
            Timestamp = DateTime.UtcNow
        };

        db.AuditLogs.Add(log);
        await db.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetLogsAsync(string? entityType = null, string? entityId = null, int take = 50)
    {
        var query = db.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (!string.IsNullOrEmpty(entityId))
            query = query.Where(a => a.EntityId == entityId);

        return await query.OrderByDescending(a => a.Timestamp).Take(take).ToListAsync();
    }
}
