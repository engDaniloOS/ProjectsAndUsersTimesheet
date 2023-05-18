using System;
using System.Threading.Tasks;
using TimeSheet.DataProviders.Repository;
using TimeSheet.Domain.Converters;
using TimeSheet.Domain.Dtos;
using TimeSheet.Domain.UseCases;

namespace TimeSheet.Domain
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _repository;
        private readonly IProjectConverter _converter;

        public ProjectService(IProjectRepository repository, IProjectConverter converter)
        {
            _repository = repository;
            _converter = converter;
        }

        public async Task<ProjectsOutDto> GetProjects()
        {
            try
            {
                var projects = await _repository.GetProjects();

                return _converter.ConvertFrom(projects);
            }
            catch (Exception ex)
            {
                var message = "Error when it tried to get projects. Error: " + ex.Message;

                Console.WriteLine(message + ex.StackTrace);
                return new ProjectsOutDto { Error = message };
            }
        }

        public async Task<ProjectWithUserOutDto> GetProjectById(int id)
        {
            try
            {
                var project = await _repository.GetProjectById(id);

                if (project == null || project.Id == 0)
                    return new ProjectWithUserOutDto();

                return _converter.ConvertToProjectWithUserFrom(project);
            }
            catch (Exception ex)
            {
                var message = $"Error when it tried to get project with id {id}. Error: {ex.Message}";

                Console.WriteLine(message + ex.StackTrace);
                return new ProjectWithUserOutDto { Error =  message };
            }
        }

        public async Task<ProjectWithUserOutDto> SaveNewProject(ProjectDto projectDto)
        {
            try
            {
                var project = _converter.ConvertFrom(projectDto);

                var savedProject = await _repository.SaveNewProject(project, projectDto.UsersIds);

                return _converter.ConvertToProjectWithUserFrom(savedProject);
            }
            catch (Exception ex)
            {
                var message = "Error when it tried to save new project. Error: " + ex.Message;

                Console.WriteLine(message + ex.StackTrace);
                return new ProjectWithUserOutDto { Error = message };
            }
        }

        public async  Task<ProjectWithUserOutDto> EditProject(int projectId, ProjectDto projectDto)
        {
            try
            {
                var project = _converter.ConvertFrom(projectDto);

                var editedProject = await _repository.EditUser(projectId, project, projectDto.UsersIds);

                return _converter.ConvertToProjectWithUserFrom(editedProject);
            }
            catch (Exception ex)
            {
                var message = "Error when it tried to edit project. Error: " + ex.Message;

                Console.WriteLine(message + ex.StackTrace);
                return new ProjectWithUserOutDto { Error = message };
            }
        }
    }
}
