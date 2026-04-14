using Microsoft.EntityFrameworkCore;
using NPA.Data;
using NPA.Data.Models;

namespace NPA.Services;

public class IssueService(NpaDbContext db, AuditService audit)
{
    public async Task<List<Issue>> GetByProjectAsync(int projectId)
    {
        return await db.Issues
            .Include(i => i.AssignedTo)
            .Include(i => i.CreatedBy)
            .Include(i => i.Project)
            .Where(i => i.ProjectId == projectId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Issue>> GetAllAsync()
    {
        return await db.Issues
            .Include(i => i.AssignedTo)
            .Include(i => i.CreatedBy)
            .Include(i => i.Project)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<Issue?> GetByIdAsync(int id)
    {
        return await db.Issues
            .Include(i => i.AssignedTo)
            .Include(i => i.CreatedBy)
            .Include(i => i.Project)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Issue> CreateAsync(Issue issue, string userId, string userName)
    {
        issue.CreatedById = userId;
        issue.CreatedAt = DateTime.UtcNow;
        db.Issues.Add(issue);
        await db.SaveChangesAsync();

        await audit.LogAsync("Issue", issue.Id.ToString(), "Created", userId, userName,
            newValues: new { issue.Title, issue.Severity, issue.Status, issue.Priority });

        return issue;
    }

    public async Task UpdateAsync(Issue issue, string userId, string userName)
    {
        issue.UpdatedAt = DateTime.UtcNow;
        if (issue.Status == IssueStatus.Resolved && issue.ResolvedAt is null)
            issue.ResolvedAt = DateTime.UtcNow;

        db.Issues.Update(issue);
        await db.SaveChangesAsync();

        await audit.LogAsync("Issue", issue.Id.ToString(), "Updated", userId, userName,
            newValues: new { issue.Title, issue.Severity, issue.Status, issue.Priority });
    }

    public async Task DeleteAsync(int id, string userId, string userName)
    {
        var issue = await db.Issues.FindAsync(id);
        if (issue is null) return;

        issue.IsDeleted = true;
        issue.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await audit.LogAsync("Issue", id.ToString(), "Deleted", userId, userName);
    }
}
