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
        private const string LoginInUse = "Login already in use";
        private const string EmailInUse = "Email already in use";
        private const string LoginOrEmailInUse = "Login or email already in use";

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
                if (string.IsNullOrWhiteSpace(userDto.Password))
                    return new UserOutDto { Error = "The password field is required." };

                var conflict = await FindConflictFor(userDto, ignoreUserId: null);

                if (conflict != null)
                    return conflict;

                var user = _converter.ConvertFrom(userDto);
                var savedUser = await _repository.SaveNewUser(user);

                return _converter.ConvertFrom(savedUser);
            }
            catch (UniqueConstraintViolationException)
            {
                return new UserOutDto { IsConflict = true, Error = LoginOrEmailInUse };
            }
            catch (Exception ex)
            {
                var message = "Error when it tried to save new user. Error: " + ex.Message;

                Console.WriteLine(message + ex.StackTrace);
                return new UserOutDto { Error = message };
            }
        }

        public async Task<UserOutDto> EditUser(int userId, UserDto userDto)
        {
            try
            {
                var existingUser = await _repository.GetUserById(userId);

                if (existingUser == null)
                    return new UserOutDto();

                if (userDto.Password != null && string.IsNullOrWhiteSpace(userDto.Password))
                    return new UserOutDto { Error = "The password field cannot be blank." };

                var conflict = await FindConflictFor(userDto, ignoreUserId: userId);

                if (conflict != null)
                    return conflict;

                var user = _converter.ConvertFrom(userDto);

                var editedUser = await _repository.EditUser(userId, user);

                return _converter.ConvertFrom(editedUser);
            }
            catch (UniqueConstraintViolationException)
            {
                return new UserOutDto { IsConflict = true, Error = LoginOrEmailInUse };
            }
            catch (Exception ex)
            {
                var message = "Error when it tried to edit user. Error: " + ex.Message;

                Console.WriteLine(message + ex.StackTrace);
                return new UserOutDto { Error = message };
            }
        }

        private async Task<UserOutDto> FindConflictFor(UserDto userDto, int? ignoreUserId)
        {
            if (await _repository.ExistsLogin(userDto.Login, ignoreUserId))
                return new UserOutDto { IsConflict = true, Error = LoginInUse };

            if (await _repository.ExistsEmail(userDto.Email, ignoreUserId))
                return new UserOutDto { IsConflict = true, Error = EmailInUse };

            return null;
        }
    }
}
