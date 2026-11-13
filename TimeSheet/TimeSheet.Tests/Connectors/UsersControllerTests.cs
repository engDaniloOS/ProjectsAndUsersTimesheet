using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Threading.Tasks;
using TimeSheet.Connectors.Controllers;
using TimeSheet.Domain.Dtos;
using TimeSheet.Domain.UseCases;
using Xunit;

namespace TimeSheet.Tests.Connectors
{
    public class UsersControllerTests
    {
        private readonly IUserService _service = Substitute.For<IUserService>();
        private readonly UsersController _controller;

        public UsersControllerTests() => _controller = new UsersController(_service);

        private static UserDto Dto() => new UserDto { Name = "Ana", Email = "ana@x.com", Login = "ana", Password = "p" };

        // ---------- POST /users ----------

        [Fact]
        public async Task SaveNewUser_Success_ReturnsOk()
        {
            _service.SaveNewUser(Arg.Any<UserDto>()).Returns(new UserOutDto { Id = 1 });

            var result = await _controller.SaveNewUser(Dto());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task SaveNewUser_Conflict_ReturnsConflictWithMessage()
        {
            _service.SaveNewUser(Arg.Any<UserDto>())
                    .Returns(new UserOutDto { IsConflict = true, Error = "Login already in use" });

            var result = await _controller.SaveNewUser(Dto());

            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal("Login already in use", Assert.IsType<ErrorDto>(conflict.Value).Message);
        }

        [Fact]
        public async Task SaveNewUser_Error_ReturnsBadRequest()
        {
            _service.SaveNewUser(Arg.Any<UserDto>()).Returns(new UserOutDto { Error = "The password field is required." });

            var result = await _controller.SaveNewUser(Dto());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("The password field is required.", Assert.IsType<ErrorDto>(badRequest.Value).Message);
        }

        // ---------- PUT /users/{ID} ----------

        [Fact]
        public async Task EditUser_Success_ReturnsOk()
        {
            _service.EditUser(1, Arg.Any<UserDto>()).Returns(new UserOutDto { Id = 1 });

            Assert.IsType<OkObjectResult>(await _controller.EditUser(1, Dto()));
        }

        [Fact]
        public async Task EditUser_Conflict_ReturnsConflict()
        {
            _service.EditUser(1, Arg.Any<UserDto>())
                    .Returns(new UserOutDto { IsConflict = true, Error = "Email already in use" });

            Assert.IsType<ConflictObjectResult>(await _controller.EditUser(1, Dto()));
        }

        [Fact]
        public async Task EditUser_MissingUser_ReturnsNotFound()
        {
            _service.EditUser(99, Arg.Any<UserDto>()).Returns(new UserOutDto());

            Assert.IsType<NotFoundResult>(await _controller.EditUser(99, Dto()));
        }

        [Fact]
        public async Task EditUser_Error_ReturnsBadRequest()
        {
            _service.EditUser(1, Arg.Any<UserDto>()).Returns(new UserOutDto { Error = "boom" });

            Assert.IsType<BadRequestObjectResult>(await _controller.EditUser(1, Dto()));
        }

        // ---------- GET /users/{ID} ----------

        [Fact]
        public async Task GetUserById_MissingUser_ReturnsNotFound()
        {
            _service.GetUserById(99).Returns(new UserWithProjectsOutDto());

            Assert.IsType<NotFoundResult>(await _controller.GetUserById(99));
        }

        [Fact]
        public async Task GetUserById_ExistingUser_ReturnsOk()
        {
            _service.GetUserById(1).Returns(new UserWithProjectsOutDto { Id = 1 });

            Assert.IsType<OkObjectResult>(await _controller.GetUserById(1));
        }
    }
}
