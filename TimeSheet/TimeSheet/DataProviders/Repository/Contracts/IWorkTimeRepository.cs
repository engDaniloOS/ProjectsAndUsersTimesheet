using System.Collections.Generic;
using System.Threading.Tasks;
using TimeSheet.Models;

namespace TimeSheet.DataProviders.Repository
{
    public interface IWorkTimeRepository
    {
        Task<WorkTime> Save(WorkTime workTime);
        Task<List<WorkTime>> GetWorkTimesByProjectId(int projectId);
        Task<WorkTime> EditWorkTime(long workTimeId, WorkTime workTime);
    }
}
