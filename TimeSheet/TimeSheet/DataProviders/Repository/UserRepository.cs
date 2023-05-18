using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TimeSheet.Models;

namespace TimeSheet.DataProviders.Repository
{
    public class UserRepository: IUserRepository
    {
        private readonly Context _context;

        public UserRepository(Context context) => _context = context;

        public async Task<User> GetUserByCredentialId(int id)
            => await _context.User.Where(u => u.Credential.Id == id).FirstOrDefaultAsync();

        public async Task<User> GetUserById(int userId)
            => await _context.User.Where(u => u.Id == userId)
                                  .Include(u => u.Credential)
                                  .Include(u => u.Projects)
                                  .FirstOrDefaultAsync();

        public async Task<User> SaveNewUser(User user)
        {
            await _context.User.AddAsync(user);

            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> EditUser(int userId, User user)
        {
            var userToEdit = await _context.User.Where(u => u.Id == userId)
                                          .Include(u => u.Credential)
                                          .FirstOrDefaultAsync();

            if (userToEdit == null || userToEdit.Id == 0)
                return new User();

            userToEdit.Name = user.Name;
            userToEdit.Email = user.Email;
            userToEdit.Credential.Login = user.Credential.Login;
            userToEdit.Credential.Password = user.Credential.Password;

            _context.Entry(userToEdit).State = EntityState.Modified;
            _context.Update(userToEdit);
            await _context.SaveChangesAsync();

            return userToEdit;
        }
    }
}
