using System.Threading.Tasks;
using TimeSheet.Domain.Dtos;

namespace TimeSheet.Domain.UseCases
{
    public interface IGetProjectById
    {
        Task<ProjectWithUserOutDto> GetProjectById(int id);

    }
}
