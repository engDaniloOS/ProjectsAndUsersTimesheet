using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TimeSheet.Models;

namespace TimeSheet.DataProviders.Repository
{
    public class CredentialRepository: ICredentialRepository
    {
        private readonly Context _context;

        public CredentialRepository(Context context) => _context = context;

        public async Task<Credential> GetAuthenticateFrom(Credential credential)
            => await _context.Credentials
                             .Where(c => c.Login.Equals(credential.Login) && c.Password.Equals(credential.Password))
                             .FirstOrDefaultAsync();
    }
}
