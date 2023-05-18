using TimeSheet.Domain.Dtos;
using TimeSheet.Models;

namespace TimeSheet.Domain.Converters
{
    public interface IUserConverter
    {
        UserWithProjectsOutDto ConvertToUserWithProjectsFrom(User user);
        User ConvertFrom(UserDto userDto);
        UserOutDto ConvertFrom(User user);
    }
}