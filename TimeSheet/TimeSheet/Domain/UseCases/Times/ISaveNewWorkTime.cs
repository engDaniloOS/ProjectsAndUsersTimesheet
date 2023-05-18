using System.Threading.Tasks;
using TimeSheet.Domain.Dtos;

namespace TimeSheet.Domain.UseCases
{
    public interface ISaveNewWorkTime
    {
        Task<WorkTimeOutDto> Save(WorkTimeDto workTime);
    }
}
