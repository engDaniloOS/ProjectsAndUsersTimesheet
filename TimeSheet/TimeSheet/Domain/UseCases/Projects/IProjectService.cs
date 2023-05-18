namespace TimeSheet.Domain.UseCases
{
    public interface IProjectService : IGetProjects, IGetProjectById, ISaveNewProject, IEditProject
    {
    }
}
