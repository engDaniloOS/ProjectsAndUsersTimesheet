using System.Threading.Tasks;
using TimeSheet.Domain.Dtos;

namespace TimeSheet.Domain.UseCases
{
    public interface IEditWorkTime
    {
        Task<WorkTimeOutDto> EditWorkTime(long workTimeId, WorkTimeDto workTimeDto);
    }
}
