using System.Threading.Tasks;
using TimeSheet.Domain.Dtos;

namespace TimeSheet.Domain.UseCases
{
    public interface IGetUserById
    {
        Task<UserWithProjectsOutDto> GetUserById(int userId);
    }
}
