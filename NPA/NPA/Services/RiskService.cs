using Microsoft.EntityFrameworkCore;
using NPA.Data;
using NPA.Data.Models;

namespace NPA.Services;

public class RiskService(NpaDbContext db, AuditService audit)
{
    public async Task<List<Risk>> GetByProjectAsync(int projectId)
    {
        return await db.Risks
            .Include(r => r.Owner)
            .Include(r => r.Project)
            .Where(r => r.ProjectId == projectId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Risk>> GetAllAsync()
    {
        return await db.Risks
            .Include(r => r.Owner)
            .Include(r => r.Project)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Risk?> GetByIdAsync(int id)
    {
        return await db.Risks
            .Include(r => r.Owner)
            .Include(r => r.Project)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Risk> CreateAsync(Risk risk, string userId, string userName)
    {
        risk.CreatedAt = DateTime.UtcNow;
        db.Risks.Add(risk);
        await db.SaveChangesAsync();

        await audit.LogAsync("Risk", risk.Id.ToString(), "Created", userId, userName,
            newValues: new { risk.Title, risk.Likelihood, risk.Impact, risk.Status });

        return risk;
    }

    public async Task UpdateAsync(Risk risk, string userId, string userName)
    {
        risk.UpdatedAt = DateTime.UtcNow;
        db.Risks.Update(risk);
        await db.SaveChangesAsync();

        await audit.LogAsync("Risk", risk.Id.ToString(), "Updated", userId, userName,
            newValues: new { risk.Title, risk.Likelihood, risk.Impact, risk.Status });
    }

    public async Task DeleteAsync(int id, string userId, string userName)
    {
        var risk = await db.Risks.FindAsync(id);
        if (risk is null) return;

        risk.IsDeleted = true;
        risk.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await audit.LogAsync("Risk", id.ToString(), "Deleted", userId, userName);
    }
}
