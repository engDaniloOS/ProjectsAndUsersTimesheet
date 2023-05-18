using System.Threading.Tasks;
using TimeSheet.Models;

namespace TimeSheet.DataProviders.Repository
{
    public interface ICredentialRepository
    {
        Task<Credential> GetAuthenticateFrom(Credential credential);
    }
}
