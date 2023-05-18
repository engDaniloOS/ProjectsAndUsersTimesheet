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
    }
}
