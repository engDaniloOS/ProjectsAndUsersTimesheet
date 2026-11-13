using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimeSheet.DataProviders.Repository;
using TimeSheet.Domain;
using TimeSheet.Domain.Converters;
using TimeSheet.Domain.Dtos;
using TimeSheet.Models;
using TimeSheet.Tests.Support;
using Xunit;

namespace TimeSheet.Tests.Domain
{
    public class UserServiceTests
    {
        private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
        private readonly UserService _service;

        public UserServiceTests()
            => _service = new UserService(_repository, new UserConverter(TestMapper.Create()));

        private static UserDto ValidDto(string password = "secret") => new UserDto
        {
            Name = "Ana",
            Email = "ana@x.com",
            Login = "ana",
            Password = password
        };

        private static User StoredUser(int id = 1) => new User
        {
            Id = id,
            Name = "Ana",
            Email = "ana@x.com",
            Credential = new Credential { Id = id, Login = "ana", Password = "secret" },
            Projects = new List<Project> { new Project { Id = 7, Title = "P", Description = "D" } }
        };

        // ---------- Cenario 1: criar usuario valido ----------

        [Fact]
        public async Task SaveNewUser_ValidUser_ReturnsSavedUser()
        {
            _repository.SaveNewUser(Arg.Any<User>()).Returns(call =>
            {
                var user = call.Arg<User>();
                user.Id = 5;
                return user;
            });

            var result = await _service.SaveNewUser(ValidDto());

            Assert.Equal(5, result.Id);
            Assert.Equal("ana", result.Login);
            Assert.Equal("ana@x.com", result.Email);
            Assert.Null(result.Error);
            Assert.False(result.IsConflict);
        }

        // ---------- Cenario 2: consultar com projetos ----------

        [Fact]
        public async Task GetUserById_ExistingUser_ReturnsUserWithProjectsAndLogin()
        {
            _repository.GetUserById(1).Returns(StoredUser());

            var result = await _service.GetUserById(1);

            Assert.Equal(1, result.Id);
            Assert.Equal("ana", result.Login);
            var project = Assert.Single(result.Projects);
            Assert.Equal(7, project.Id);
        }

        // ---------- Cenario 3: id inexistente ----------

        [Fact]
        public async Task GetUserById_MissingUser_ReturnsEmptyDto()
        {
            _repository.GetUserById(99).Returns((User)null);

            var result = await _service.GetUserById(99);

            Assert.Equal(0, result.Id);
            Assert.Null(result.Error);
        }

        [Fact]
        public async Task EditUser_MissingUser_ReturnsEmptyDtoEvenIfLoginExistsElsewhere()
        {
            _repository.GetUserById(99).Returns((User)null);
            _repository.ExistsLogin("ana", 99).Returns(true);
            _repository.ExistsEmail("ana@x.com", 99).Returns(true);

            var result = await _service.EditUser(99, ValidDto());

            Assert.Equal(0, result.Id);
            Assert.False(result.IsConflict);
            Assert.Null(result.Error);
            await _repository.DidNotReceive().EditUser(Arg.Any<int>(), Arg.Any<User>());
        }

        // ---------- Cenario 4: unicidade ----------

        [Fact]
        public async Task SaveNewUser_LoginAlreadyInUse_ReturnsConflictAndDoesNotSave()
        {
            _repository.ExistsLogin("ana", null).Returns(true);

            var result = await _service.SaveNewUser(ValidDto());

            Assert.True(result.IsConflict);
            Assert.Equal("Login already in use", result.Error);
            await _repository.DidNotReceive().SaveNewUser(Arg.Any<User>());
        }

        [Fact]
        public async Task SaveNewUser_EmailAlreadyInUse_ReturnsConflictAndDoesNotSave()
        {
            _repository.ExistsEmail("ana@x.com", null).Returns(true);

            var result = await _service.SaveNewUser(ValidDto());

            Assert.True(result.IsConflict);
            Assert.Equal("Email already in use", result.Error);
            await _repository.DidNotReceive().SaveNewUser(Arg.Any<User>());
        }

        [Fact]
        public async Task SaveNewUser_RepositoryReportsUniqueViolation_ReturnsConflict()
        {
            _repository.SaveNewUser(Arg.Any<User>())
                       .ThrowsAsync(new UniqueConstraintViolationException("duplicate", new Exception()));

            var result = await _service.SaveNewUser(ValidDto());

            Assert.True(result.IsConflict);
            Assert.False(string.IsNullOrWhiteSpace(result.Error));
        }

        [Fact]
        public async Task EditUser_LoginBelongsToAnotherUser_ReturnsConflict()
        {
            _repository.GetUserById(1).Returns(StoredUser());
            _repository.ExistsLogin("ana", 1).Returns(true);

            var result = await _service.EditUser(1, ValidDto());

            Assert.True(result.IsConflict);
            Assert.Equal("Login already in use", result.Error);
            await _repository.DidNotReceive().EditUser(Arg.Any<int>(), Arg.Any<User>());
        }

        [Fact]
        public async Task EditUser_EmailBelongsToAnotherUser_ReturnsConflict()
        {
            _repository.GetUserById(1).Returns(StoredUser());
            _repository.ExistsEmail("ana@x.com", 1).Returns(true);

            var result = await _service.EditUser(1, ValidDto());

            Assert.True(result.IsConflict);
            Assert.Equal("Email already in use", result.Error);
        }

        [Fact]
        public async Task EditUser_ChecksUniquenessIgnoringTheUsersOwnId()
        {
            _repository.GetUserById(1).Returns(StoredUser());
            _repository.EditUser(1, Arg.Any<User>()).Returns(StoredUser());

            var result = await _service.EditUser(1, ValidDto());

            Assert.False(result.IsConflict);
            Assert.Null(result.Error);
            await _repository.Received(1).ExistsLogin("ana", 1);
            await _repository.Received(1).ExistsEmail("ana@x.com", 1);
        }

        [Fact]
        public async Task EditUser_RepositoryReportsUniqueViolation_ReturnsConflict()
        {
            _repository.GetUserById(1).Returns(StoredUser());
            _repository.EditUser(1, Arg.Any<User>())
                       .ThrowsAsync(new UniqueConstraintViolationException("duplicate", new Exception()));

            var result = await _service.EditUser(1, ValidDto());

            Assert.True(result.IsConflict);
        }

        // ---------- Cenarios 5 e 6: senha ----------

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SaveNewUser_MissingPassword_ReturnsErrorAndDoesNotSave(string password)
        {
            var result = await _service.SaveNewUser(ValidDto(password));

            Assert.Equal("The password field is required.", result.Error);
            Assert.False(result.IsConflict);
            await _repository.DidNotReceive().SaveNewUser(Arg.Any<User>());
        }

        [Fact]
        public async Task EditUser_WithoutPassword_SucceedsAndSendsNullPasswordToKeepTheCurrentOne()
        {
            _repository.GetUserById(1).Returns(StoredUser());
            _repository.EditUser(1, Arg.Any<User>()).Returns(StoredUser());

            var result = await _service.EditUser(1, ValidDto(password: null));

            Assert.Null(result.Error);
            Assert.Equal(1, result.Id);
            await _repository.Received(1).EditUser(1, Arg.Is<User>(u => u.Credential.Password == null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task EditUser_BlankPassword_ReturnsErrorAndDoesNotEdit(string password)
        {
            _repository.GetUserById(1).Returns(StoredUser());

            var result = await _service.EditUser(1, ValidDto(password));

            Assert.Equal("The password field cannot be blank.", result.Error);
            await _repository.DidNotReceive().EditUser(Arg.Any<int>(), Arg.Any<User>());
        }

        // ---------- Erros inesperados ----------

        [Fact]
        public async Task SaveNewUser_UnexpectedError_ReturnsErrorMessageWithoutConflict()
        {
            _repository.SaveNewUser(Arg.Any<User>()).ThrowsAsync(new InvalidOperationException("boom"));

            var result = await _service.SaveNewUser(ValidDto());

            Assert.StartsWith("Error when it tried to save new user.", result.Error);
            Assert.False(result.IsConflict);
        }

        [Fact]
        public async Task EditUser_UnexpectedError_ReturnsErrorMessageWithoutConflict()
        {
            _repository.GetUserById(1).Returns(StoredUser());
            _repository.EditUser(1, Arg.Any<User>()).ThrowsAsync(new InvalidOperationException("boom"));

            var result = await _service.EditUser(1, ValidDto());

            Assert.StartsWith("Error when it tried to edit user.", result.Error);
            Assert.False(result.IsConflict);
        }
    }
}
