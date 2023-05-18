using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TimeSheet.Domain.Dtos;
using TimeSheet.Domain.UseCases;

namespace TimeSheet.Connectors.Controllers
{
    [Route("api/v1/users")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service) => _service = service;

        [HttpGet]
        [Route("{ID}")]
        public async Task<IActionResult> GetUserById([FromRoute(Name = "ID")] int userId)
        {
            var user = await _service.GetUserById(userId);

            if (!string.IsNullOrWhiteSpace(user.Error))
                return BadRequest(new ErrorDto { Message = user.Error });

            if (user.Id == 0)
                return NotFound();

            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> SaveNewUser([FromBody] UserDto userDto)
        {
            var user = await _service.SaveNewUser(userDto);

            if (!string.IsNullOrWhiteSpace(user.Error))
                return BadRequest(new ErrorDto { Message = user.Error });

            return Ok(user);
        }

        [HttpPut]
        [Route("{ID}")]
        public async Task<IActionResult> EditUser([FromRoute(Name = "ID")] int userId, [FromBody] UserDto userDto)
        {
            var user = await _service.EditUser(userId, userDto);

            if (!string.IsNullOrWhiteSpace(user.Error))
                return BadRequest(new ErrorDto { Message = user.Error });

            if (user.Id == 0)
                return NotFound();

            return Ok(user);
        }
    }
}
