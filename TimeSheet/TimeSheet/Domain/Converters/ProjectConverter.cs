using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using TimeSheet.Domain.Dtos;
using TimeSheet.Models;

namespace TimeSheet.Domain.Converters
{
    public class ProjectConverter : IProjectConverter
    {
        private readonly IMapper _mapper;

        public ProjectConverter(IMapper mapper) => _mapper = mapper;

        public ProjectsOutDto ConvertFrom(List<Project> projects)
        {
            var projectDtos = new List<ProjectWithUserOutDto>();

            foreach (var project in projects)
            {
                var dto = _mapper.Map<Project, ProjectWithUserOutDto>(project);

                foreach (var userDto in dto.Users)
                {
                    var user = project.Users.Where(u => u.Id == userDto.Id).FirstOrDefault();
                    userDto.Login = user.Credential.Login;
                }

                projectDtos.Add(dto);
            }

            return new ProjectsOutDto { Projects = projectDtos };
        }

        public ProjectWithUserOutDto ConvertToProjectWithUserFrom(Project project)
        {
            var projectDto = _mapper.Map<Project, ProjectWithUserOutDto>(project);
            var users = new List<UserOutDto>();

            foreach (var user in projectDto.Users)
            {
                var originalUser = project.Users.Where(u => u.Id == user.Id).FirstOrDefault();
                
                user.Login = originalUser.Credential.Login;
                users.Add(user);
            }

            projectDto.Users = users;

            return projectDto;
        }

        public Project ConvertFrom(ProjectDto projectDto)
        {
            var project = _mapper.Map<ProjectDto, Project>(projectDto);
            project.Users = new List<User>();

            return project;
        }
    }
}
