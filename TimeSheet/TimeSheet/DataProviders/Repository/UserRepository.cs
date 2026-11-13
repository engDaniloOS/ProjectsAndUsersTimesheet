using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TimeSheet.Models;

namespace TimeSheet.DataProviders.Repository
{
    public class UserRepository: IUserRepository
    {
        // Números de erro do SQL Server: 2601 = índice único violado, 2627 = constraint única/PK violada.
        private static readonly int[] SqlServerUniqueViolationNumbers = { 2601, 2627 };

        private readonly Context _context;

        public UserRepository(Context context) => _context = context;

        public async Task<User> GetUserByCredentialId(int id)
            => await _context.User.Where(u => u.Credential.Id == id).FirstOrDefaultAsync();

        public async Task<bool> ExistsLogin(string login, int? ignoreUserId)
            => await _context.User.AnyAsync(u => u.Credential.Login == login
                                                 && (ignoreUserId == null || u.Id != ignoreUserId));

        public async Task<bool> ExistsEmail(string email, int? ignoreUserId)
            => await _context.User.AnyAsync(u => u.Email == email
                                                 && (ignoreUserId == null || u.Id != ignoreUserId));

        public async Task<User> GetUserById(int userId)
            => await _context.User.Where(u => u.Id == userId)
                                  .Include(u => u.Credential)
                                  .Include(u => u.Projects)
                                  .FirstOrDefaultAsync();

        public async Task<User> SaveNewUser(User user)
        {
            await _context.User.AddAsync(user);

            await SaveChangesTranslatingUniqueViolation();

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

            // Senha opcional na edição: só é alterada quando informada.
            if (user.Credential.Password != null)
                userToEdit.Credential.Password = user.Credential.Password;

            _context.Entry(userToEdit).State = EntityState.Modified;
            _context.Update(userToEdit);
            await SaveChangesTranslatingUniqueViolation();

            return userToEdit;
        }

        private async Task SaveChangesTranslatingUniqueViolation()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                throw new UniqueConstraintViolationException("Login or email already in use.", ex);
            }
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
            => ex.InnerException is SqlException sqlException
               && SqlServerUniqueViolationNumbers.Contains(sqlException.Number);
    }
}
