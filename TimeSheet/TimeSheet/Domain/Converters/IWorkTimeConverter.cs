using System.Collections.Generic;
using TimeSheet.Domain.Dtos;
using TimeSheet.Models;

namespace TimeSheet.Domain.Converters
{
    public interface IWorkTimeConverter
    {
        WorkTimeOutDto ConvertFrom(WorkTime workTime);
        WorkTime ConvertFrom(WorkTimeDto workTimeDto);
        WorkTimesOutDto ConvertFrom(List<WorkTime> workTimes);
    }
}