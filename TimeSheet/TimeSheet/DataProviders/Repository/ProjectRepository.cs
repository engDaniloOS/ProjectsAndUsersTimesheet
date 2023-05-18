using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSheet.Models;

namespace TimeSheet.DataProviders.Repository
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly Context _context;

        public ProjectRepository(Context context) => _context = context;

        public async Task<List<Project>> GetProjects()
            => await _context.Project
                             .Include(p => p.Users)
                             .ThenInclude(u => u.Credential)
                             .ToListAsync();

        public async Task<Project> GetProjectById(int id)
            => await _context.Project.Where(p => p.Id == id)
                                     .Include(p => p.Users)
                                     .ThenInclude(u => u.Credential)
                                     .FirstOrDefaultAsync();

        public async Task<Project> SaveNewProject(Project project, List<int> usersIds)
        {
            using var transaction = _context.Database.BeginTransaction();

            foreach (var userId in usersIds)
                project.Users.Add(await _context.User.Where(u => u.Id == userId).FirstOrDefaultAsync());

            await _context.Project.AddAsync(project);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetProjectById(project.Id);
        }

        public async Task<Project> EditUser(int projectId, Project project, List<int> usersIds)
        {
            using var transaction = _context.Database.BeginTransaction();

            var projectToEdit = await _context.Project.Where(p => p.Id == projectId)
                                                      .Include(p => p.Users)
                                                      .FirstOrDefaultAsync();

            if (projectToEdit == null || projectToEdit.Id == 0)
                return new Project();

            projectToEdit.Title = project.Title;
            projectToEdit.Description = project.Description;
            projectToEdit.Users.Clear();

            foreach (var userId in usersIds)
                projectToEdit.Users.Add(await _context.User.Where(u => u.Id == userId).FirstOrDefaultAsync());

            _context.Entry(projectToEdit).State = EntityState.Modified;
            _context.Update(projectToEdit);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetProjectById(projectToEdit.Id);
        }
    }
}
