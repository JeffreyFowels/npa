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
            .Include(p => p.Milestones).ThenInclude(m => m.Tasks).ThenInclude(t => t.Consultant)
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
            .Include(p => p.Milestones).ThenInclude(m => m.Tasks).ThenInclude(t => t.Consultant)
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
            .Include(p => p.Milestones.OrderBy(m => m.SortOrder))
                .ThenInclude(m => m.Tasks.OrderBy(t => t.SortOrder))
                .ThenInclude(t => t.Consultant)
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

    // ── Milestone CRUD ──

    public async Task<Milestone> AddMilestoneAsync(int projectId, string name, string userId, string userName)
    {
        var maxSort = await db.Milestones.Where(m => m.ProjectId == projectId).MaxAsync(m => (int?)m.SortOrder) ?? 0;
        var milestone = new Milestone
        {
            ProjectId = projectId,
            Name = name,
            SortOrder = maxSort + 1,
            CreatedAt = DateTime.UtcNow
        };
        db.Milestones.Add(milestone);
        await db.SaveChangesAsync();
        await audit.LogAsync("Milestone", milestone.Id.ToString(), "Created", userId, userName,
            newValues: new { milestone.Name, milestone.ProjectId });
        return milestone;
    }

    public async Task UpdateMilestoneAsync(Milestone milestone, string userId, string userName)
    {
        db.Milestones.Update(milestone);
        await db.SaveChangesAsync();
        await audit.LogAsync("Milestone", milestone.Id.ToString(), "Updated", userId, userName,
            newValues: new { milestone.Name });
    }

    public async Task DeleteMilestoneAsync(int milestoneId, string userId, string userName)
    {
        var milestone = await db.Milestones.FindAsync(milestoneId);
        if (milestone is null) return;
        milestone.IsDeleted = true;
        await db.SaveChangesAsync();
        await audit.LogAsync("Milestone", milestoneId.ToString(), "Deleted", userId, userName);
    }

    // ── Task CRUD ──

    public async Task<ProjectTask> AddTaskAsync(ProjectTask task, string userId, string userName)
    {
        var maxSort = await db.ProjectTasks.Where(t => t.MilestoneId == task.MilestoneId).MaxAsync(t => (int?)t.SortOrder) ?? 0;
        task.SortOrder = maxSort + 1;
        task.CreatedAt = DateTime.UtcNow;
        db.ProjectTasks.Add(task);
        await db.SaveChangesAsync();
        await audit.LogAsync("ProjectTask", task.Id.ToString(), "Created", userId, userName,
            newValues: new { task.TaskName, task.MilestoneId, task.Priority, task.Status });
        return task;
    }

    public async Task UpdateTaskAsync(ProjectTask task, string userId, string userName)
    {
        task.UpdatedAt = DateTime.UtcNow;
        if (task.Status == Data.Models.TaskStatus.Complete && task.CompletionDate is null)
            task.CompletionDate = DateTime.UtcNow;
        db.ProjectTasks.Update(task);
        await db.SaveChangesAsync();
        await audit.LogAsync("ProjectTask", task.Id.ToString(), "Updated", userId, userName,
            newValues: new { task.TaskName, task.Status, task.PercentComplete });
    }

    public async Task DeleteTaskAsync(int taskId, string userId, string userName)
    {
        var task = await db.ProjectTasks.FindAsync(taskId);
        if (task is null) return;
        task.IsDeleted = true;
        task.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await audit.LogAsync("ProjectTask", taskId.ToString(), "Deleted", userId, userName);
    }

    public async Task<ProjectTask?> GetTaskByIdAsync(int taskId)
    {
        return await db.ProjectTasks
            .Include(t => t.Consultant)
            .Include(t => t.Milestone)
            .FirstOrDefaultAsync(t => t.Id == taskId);
    }

    /// <summary>Recalculate project % complete from milestone tasks.</summary>
    public async Task RecalculateProgressAsync(int projectId)
    {
        var tasks = await db.ProjectTasks
            .Where(t => t.Milestone!.ProjectId == projectId)
            .ToListAsync();

        var project = await db.Projects.FindAsync(projectId);
        if (project is null) return;

        project.PercentComplete = tasks.Count == 0 ? 0 : (int)tasks.Average(t => t.PercentComplete);
        project.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
}
