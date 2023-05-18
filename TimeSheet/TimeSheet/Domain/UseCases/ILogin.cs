using System.Threading.Tasks;
using TimeSheet.Domain.Dtos;

namespace TimeSheet.Domain.UseCases
{
    public interface ILogin
    {
        Task<CredentialOutDto> Login(CredentialDto credentialDto);
    }
}
