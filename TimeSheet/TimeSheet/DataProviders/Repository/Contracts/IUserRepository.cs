using System.Threading.Tasks;
using TimeSheet.Models;

namespace TimeSheet.DataProviders.Repository
{
    public interface IUserRepository
    {
        Task<User> GetUserByCredentialId(int id);
        Task<User> GetUserById(int userId);
        Task<User> SaveNewUser(User user);
        Task<User> EditUser(int userId, User user);
        Task<bool> ExistsLogin(string login, int? ignoreUserId);
        Task<bool> ExistsEmail(string email, int? ignoreUserId);
    }
}
