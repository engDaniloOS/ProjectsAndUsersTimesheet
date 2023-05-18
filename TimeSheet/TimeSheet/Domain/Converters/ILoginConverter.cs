using TimeSheet.Domain.Dtos;
using TimeSheet.Models;

namespace TimeSheet.Domain.Converters
{
    public interface ILoginConverter
    {
        Credential ConvertFrom(CredentialDto credentialDto);
    }
}