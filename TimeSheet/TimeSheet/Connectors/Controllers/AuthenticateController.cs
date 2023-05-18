using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TimeSheet.Domain.Dtos;
using TimeSheet.Domain.UseCases;

namespace TimeSheet.Connectors.Controllers
{
    [Route("api/v1/authenticate")]
    [ApiController]
    [AllowAnonymous]
    public class AuthenticateController : ControllerBase
    {
        private readonly ILogin _service;

        public AuthenticateController(ILogin service) => _service = service;

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] CredentialDto credential)
        {
            CredentialOutDto authenticate = await _service.Login(credential);

            if (!string.IsNullOrWhiteSpace(authenticate.Error))
                return BadRequest(new ErrorDto{ Message = "Error. Try again in some minutes. " + authenticate.Error});

            if (authenticate.IsCredentialInvalid)
                return Unauthorized(new ErrorDto { Message = "Invalid credentials"});

            return Ok(authenticate);
        }
    }
}
