using System.Threading.Tasks;
using TimeSheet.Domain.Dtos;

namespace TimeSheet.Domain.UseCases
{
    public interface ISaveNewUser
    {
        Task<UserOutDto> SaveNewUser(UserDto userDto);
    }
}
