using System.Threading.Tasks;
using TimeSheet.Domain.Dtos;

namespace TimeSheet.Domain.UseCases
{
    public interface ISaveNewProject
    {
        public Task<ProjectWithUserOutDto> SaveNewProject(ProjectDto projectDto);
    }
}
