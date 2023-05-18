using System.Collections.Generic;
using System.Threading.Tasks;
using TimeSheet.Models;

namespace TimeSheet.DataProviders.Repository
{
    public interface IProjectRepository
    {
        Task<List<Project>> GetProjects();
        Task<Project> GetProjectById(int id);
        Task<Project> SaveNewProject(Project project, List<int> usersIds);
        Task<Project> EditUser(int projectId, Project projectDto, List<int> usersIds);
    }
}
