namespace TimeSheet.Domain.UseCases
{
    public interface IWorkTimeService: IGetWorkTimeById, IGetWorkTimesByProject, ISaveNewWorkTime, IEditWorkTime
    {
    }
}
