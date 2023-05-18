using AutoMapper;
using TimeSheet.Domain.Dtos;
using TimeSheet.Models;

namespace TimeSheet.Configuration
{
    public class MapperProfile: Profile
    {
        public MapperProfile()
        {
            CreateMap<WorkTime, WorkTimeOutDto>();
            CreateMap<WorkTimeDto, WorkTime>();

            CreateMap<CredentialDto, Credential>();
            CreateMap<UserDto, Credential>();

            CreateMap<User, UserWithProjectsOutDto>();
            CreateMap<User, UserOutDto>();
            CreateMap<UserDto, User>();

            CreateMap<Project, ProjectWithUserOutDto>();
            CreateMap<Project, ProjectOutDto>();
            CreateMap<ProjectDto, Project>();
        }
    }
}
