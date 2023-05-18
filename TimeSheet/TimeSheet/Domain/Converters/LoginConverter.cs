using AutoMapper;
using TimeSheet.Domain.Dtos;
using TimeSheet.Models;

namespace TimeSheet.Domain.Converters
{
    public class LoginConverter : ILoginConverter
    {
        private readonly IMapper _mapper;

        public LoginConverter(IMapper mapper) => _mapper = mapper;

        public Credential ConvertFrom(CredentialDto credentialDto)
            => _mapper.Map<CredentialDto, Credential>(credentialDto);
    }
}
