using System.Threading.Tasks;
using TimeSheet.Domain.Dtos;

namespace TimeSheet.Domain.UseCases
{
    public interface IEditProject
    {
        Task<ProjectWithUserOutDto> EditProject(int projectId, ProjectDto projectDto);
    }
}
