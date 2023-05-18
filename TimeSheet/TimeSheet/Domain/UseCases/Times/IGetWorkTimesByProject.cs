using System.Threading.Tasks;
using TimeSheet.Domain.Dtos;

namespace TimeSheet.Domain.UseCases
{
    public interface IGetWorkTimesByProject
    {
        Task<WorkTimesOutDto> GetWorkTimesByProjectId(int projectId);
    }
}
