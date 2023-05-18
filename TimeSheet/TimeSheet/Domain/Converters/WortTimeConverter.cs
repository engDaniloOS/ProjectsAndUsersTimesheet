using AutoMapper;
using System.Collections.Generic;
using TimeSheet.Domain.Dtos;
using TimeSheet.Models;

namespace TimeSheet.Domain.Converters
{
    public class WortTimeConverter : IWorkTimeConverter
    {
        private readonly IMapper _mapper;

        public WortTimeConverter(IMapper mapper) => _mapper = mapper;

        public WorkTimeOutDto ConvertFrom(WorkTime workTime)
        {
            var workTimeDto = _mapper.Map<WorkTime, WorkTimeOutDto>(workTime);

            workTimeDto.User.Login = workTime.User.Credential.Login;

            return workTimeDto;
        }

        public WorkTime ConvertFrom(WorkTimeDto workTimeDto)
            => _mapper.Map<WorkTimeDto, WorkTime>(workTimeDto);

        public WorkTimesOutDto ConvertFrom(List<WorkTime> workTimes)
        {
            var workTimesDto = new List<WorkTimeOutDto>();

            foreach (var worktime in workTimes)
                workTimesDto.Add(ConvertFrom(worktime));

            return new WorkTimesOutDto { WorkTimes = workTimesDto };
        }
    }
}
