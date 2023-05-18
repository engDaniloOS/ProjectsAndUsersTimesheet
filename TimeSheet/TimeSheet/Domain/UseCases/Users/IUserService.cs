namespace TimeSheet.Domain.UseCases
{
    public interface IUserService: IGetUserById, ISaveNewUser, IEditUser
    {
    }
}
