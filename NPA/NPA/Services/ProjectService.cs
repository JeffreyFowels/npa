using Microsoft.EntityFrameworkCore;
using NPA.Data;
using NPA.Data.Models;

namespace NPA.Services;

public class ProjectService(NpaDbContext db, AuditService audit)
{
    public async Task<List<Project>> GetAllAsync()
    {
        return await db.Projects
            .Include(p => p.CreatedBy)
            .Include(p => p.Assignments).ThenInclude(a => a.User)
            .Include(p => p.Risks)
            .Include(p => p.Issues)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Project>> GetByUserAsync(string userId)
    {
        return await db.Projects
            .Include(p => p.CreatedBy)
            .Include(p => p.Assignments).ThenInclude(a => a.User)
            .Include(p => p.Risks)
            .Include(p => p.Issues)
            .Where(p => p.Assignments.Any(a => a.UserId == userId) || p.CreatedById == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        return await db.Projects
            .Include(p => p.CreatedBy)
            .Include(p => p.Assignments).ThenInclude(a => a.User)
            .Include(p => p.Risks)
            .Include(p => p.Issues)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Project> CreateAsync(Project project, string userId, string userName)
    {
        project.CreatedById = userId;
        project.CreatedAt = DateTime.UtcNow;
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        await audit.LogAsync("Project", project.Id.ToString(), "Created", userId, userName,
            newValues: new { project.Name, project.Status, project.Priority });

        return project;
    }

    public async Task UpdateAsync(Project project, string userId, string userName)
    {
        project.UpdatedAt = DateTime.UtcNow;
        db.Projects.Update(project);
        await db.SaveChangesAsync();

        await audit.LogAsync("Project", project.Id.ToString(), "Updated", userId, userName,
            newValues: new { project.Name, project.Status, project.Priority, project.PercentComplete });
    }

    public async Task DeleteAsync(int id, string userId, string userName)
    {
        var project = await db.Projects.FindAsync(id);
        if (project is null) return;

        project.IsDeleted = true;
        project.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        await audit.LogAsync("Project", id.ToString(), "Deleted", userId, userName);
    }

    public async Task AssignUserAsync(int projectId, string targetUserId, string assignedById, string assignedByName)
    {
        var exists = await db.ProjectAssignments
            .AnyAsync(pa => pa.ProjectId == projectId && pa.UserId == targetUserId);

        if (exists) return;

        db.ProjectAssignments.Add(new ProjectAssignment
        {
            ProjectId = projectId,
            UserId = targetUserId,
            AssignedById = assignedById,
            AssignedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        await audit.LogAsync("ProjectAssignment", projectId.ToString(), "UserAssigned", assignedById, assignedByName,
            newValues: new { ProjectId = projectId, UserId = targetUserId });
    }

    public async Task UnassignUserAsync(int projectId, string targetUserId, string userId, string userName)
    {
        var assignment = await db.ProjectAssignments
            .FirstOrDefaultAsync(pa => pa.ProjectId == projectId && pa.UserId == targetUserId);

        if (assignment is null) return;

        db.ProjectAssignments.Remove(assignment);
        await db.SaveChangesAsync();

        await audit.LogAsync("ProjectAssignment", projectId.ToString(), "UserUnassigned", userId, userName,
            newValues: new { ProjectId = projectId, UserId = targetUserId });
    }
}
