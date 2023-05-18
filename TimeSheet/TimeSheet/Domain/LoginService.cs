using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TimeSheet.DataProviders.Repository;
using TimeSheet.Domain.Converters;
using TimeSheet.Domain.Dtos;
using TimeSheet.Domain.UseCases;
using TimeSheet.Models;

namespace TimeSheet.Domain
{
    public class LoginService : ILogin
    {
        private readonly IConfiguration _configuration;
        private readonly ICredentialRepository _credentialRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILoginConverter _converter;

        public LoginService(IConfiguration configuration, 
                            ICredentialRepository credentialRepository, 
                            IUserRepository userRepository,
                            ILoginConverter converter)
        {
            _configuration = configuration;
            _credentialRepository = credentialRepository;
            _userRepository = userRepository;
            _converter = converter;
        }

        public async Task<CredentialOutDto> Login(CredentialDto credentialDto)
        {
            try
            {
                var credential = _converter.ConvertFrom(credentialDto);

                var authenticated = await _credentialRepository.GetAuthenticateFrom(credential);

                if (authenticated == null)
                    return new CredentialOutDto { IsCredentialInvalid = true };

                var user = await _userRepository.GetUserByCredentialId(authenticated.Id);

                return  new CredentialOutDto
                {
                    Token = GetTokenFrom(user),
                    User = new UserOutDto
                    {
                        Email = user.Email,
                        Id = user.Id,
                        Login = user.Credential.Login,
                        Name = user.Name
                    }
                };
            }
            catch (Exception ex)
            {
                var message = "It wasn't possible to login. Error: " + ex.Message;

                Console.WriteLine(message + ex.StackTrace);
                return new CredentialOutDto { Error =  message };
            }
        }

        private string GetTokenFrom(User user)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, user.Name), new Claim(ClaimTypes.Email, user.Email) };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Authentication")["SecurityKey"]));

            var cript = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresIn = double.Parse(_configuration.GetSection("Authentication")["ExpiresIn"]);

            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("Authentication")["Issuer"],
                audience: _configuration.GetSection("Authentication")["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(expiresIn),
                signingCredentials: cript
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
