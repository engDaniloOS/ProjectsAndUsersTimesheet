using System.Collections.Generic;
using TimeSheet.Domain.Dtos;
using TimeSheet.Models;

namespace TimeSheet.Domain.Converters
{
    public interface IProjectConverter
    {
        ProjectsOutDto ConvertFrom(List<Project> projects);
        ProjectWithUserOutDto ConvertToProjectWithUserFrom(Project project);
        Project ConvertFrom(ProjectDto projectDto);
    }
}