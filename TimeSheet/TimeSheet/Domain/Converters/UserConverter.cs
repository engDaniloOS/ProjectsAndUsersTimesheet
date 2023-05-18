using AutoMapper;
using TimeSheet.Domain.Dtos;
using TimeSheet.Models;

namespace TimeSheet.Domain.Converters
{
    public class UserConverter : IUserConverter
    {
        private readonly IMapper _mapper;

        public UserConverter(IMapper mapper) => _mapper = mapper;

        public UserWithProjectsOutDto ConvertToUserWithProjectsFrom(User user)
        {
            var userDto =  _mapper.Map<User, UserWithProjectsOutDto>(user);

            if (userDto.Id == 0)
                return userDto;

            userDto.Login = user.Credential.Login;

            return userDto;
        }

        public UserOutDto ConvertFrom(User user)
        {
            var userDto = _mapper.Map<User, UserOutDto>(user);

            if (userDto.Id == 0)
                return userDto;

            userDto.Login = user.Credential.Login;

            return userDto;
        }

        public User ConvertFrom(UserDto userDto)
        {
            var user = _mapper.Map<UserDto, User>(userDto);
            var credential = _mapper.Map<UserDto, Credential>(userDto);

            user.Credential = credential;

            return user;
        }
    }
}
