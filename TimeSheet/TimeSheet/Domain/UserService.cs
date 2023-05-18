using System;
using System.Threading.Tasks;
using TimeSheet.DataProviders.Repository;
using TimeSheet.Domain.Converters;
using TimeSheet.Domain.Dtos;
using TimeSheet.Domain.UseCases;

namespace TimeSheet.Domain
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IUserConverter _converter;

        public UserService(IUserRepository repository, IUserConverter converter)
        {
            _repository = repository;
            _converter = converter;
        }

        public async Task<UserWithProjectsOutDto> GetUserById(int userId)
        {
            try
            {
                var user = await _repository.GetUserById(userId);

                if (user == null) return new UserWithProjectsOutDto();

                return _converter.ConvertToUserWithProjectsFrom(user);
            }
            catch (Exception ex)
            {
                var message = "Error when it tried to get user. Error: " + ex.Message;

                Console.WriteLine(message + ex.StackTrace);
                return new UserWithProjectsOutDto { Error = message };
            }
        }

        public async Task<UserOutDto> SaveNewUser(UserDto userDto)
        {
            try
            {
                var user = _converter.ConvertFrom(userDto);
                var savedUser = await _repository.SaveNewUser(user);

                return _converter.ConvertFrom(savedUser);
            }
            catch (Exception ex)
            {
                var message = "Erro when it tried to save new user. Error: " + ex.Message;

                Console.WriteLine(message + ex.StackTrace);
                return new UserOutDto { Error = message };
            }
        }

        public async Task<UserOutDto> EditUser(int userId, UserDto userDto)
        {
            try
            {
                var user = _converter.ConvertFrom(userDto);

                var editedUser = await _repository.EditUser(userId, user);

                return _converter.ConvertFrom(editedUser);
            }
            catch (Exception ex)
            {
                var message = "Erro when it tried to edit user. Error: " + ex.Message;

                Console.WriteLine(message + ex.StackTrace);
                return new UserOutDto { Error =  message };
            }
        }
    }
}
