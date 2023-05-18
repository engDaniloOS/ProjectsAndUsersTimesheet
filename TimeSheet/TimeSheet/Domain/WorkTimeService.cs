using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSheet.DataProviders.Repository;
using TimeSheet.Domain.Converters;
using TimeSheet.Domain.Dtos;
using TimeSheet.Domain.UseCases;
using TimeSheet.Models;

namespace TimeSheet.Domain
{
    public class WorkTimeService : IWorkTimeService
    {
        private readonly IWorkTimeRepository _timesRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IWorkTimeConverter _converter;

        public WorkTimeService(IWorkTimeRepository repository, IWorkTimeConverter converter, IProjectRepository projectRepository)
        {
            _timesRepository = repository;
            _converter = converter;
            _projectRepository = projectRepository;
        }

        public async Task<WorkTimeOutDto> Save(WorkTimeDto workTimeDto)
        {
            try
            {
                var project = await _projectRepository.GetProjectById(workTimeDto.ProjectId);

                if (project == null || project.Id == 0 || project.Users.Count == 0)
                    return new WorkTimeOutDto();

                var user = project.Users.Where(u => u.Id == workTimeDto.UserId).FirstOrDefault();

                if (user == null || user.Id == 0)
                    return new WorkTimeOutDto();

                var workTime = _converter.ConvertFrom(workTimeDto);
                var savedWorkTime = await _timesRepository.Save(workTime);

                return _converter.ConvertFrom(savedWorkTime);
            }
            catch (Exception ex)
            {
                var message = "It wasn't possible to register work time. Error: " + ex.Message;

                Console.WriteLine(message + ex.StackTrace);
                return new WorkTimeOutDto { Error = message };
            }
        }

        public async Task<WorkTimesOutDto> GetWorkTimesByProjectId(int projectId)
        {
            try
            {
                List<WorkTime> times = await _timesRepository.GetWorkTimesByProjectId(projectId);

                return _converter.ConvertFrom(times);
            }
            catch (Exception ex)
            {
                var message = $"Error when it tried to get work times by project id {projectId}. Error: {ex.Message}";

                Console.WriteLine(message + ex.StackTrace);
                return new WorkTimesOutDto { Error =  message };
            }
        }

        public async Task<WorkTimeOutDto> EditWorkTime(long workTimeId, WorkTimeDto workTimeDto)
        {
            try
            {
                var workTime = _converter.ConvertFrom(workTimeDto);

                var editedWorkTime = await _timesRepository.EditWorkTime(workTimeId, workTime);

                return _converter.ConvertFrom(editedWorkTime);
            }
            catch (Exception ex)
            {
                var message = "Erro when it tried to edit work time. Error: " + ex.Message;

                Console.WriteLine(message + ex.StackTrace);
                return new WorkTimeOutDto { Error =  message };
            }
        }
    }
}
