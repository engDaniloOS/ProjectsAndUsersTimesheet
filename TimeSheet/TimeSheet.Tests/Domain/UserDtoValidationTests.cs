using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TimeSheet.Domain.Dtos;
using Xunit;

namespace TimeSheet.Tests.Domain
{
    public class UserDtoValidationTests
    {
        private static UserDto Valid() => new UserDto
        {
            Name = "Ana",
            Email = "ana@x.com",
            Login = "ana",
            Password = "secret"
        };

        private static List<ValidationResult> Validate(UserDto dto)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, new ValidationContext(dto), results, validateAllProperties: true);
            return results;
        }

        private static bool Invalid(List<ValidationResult> results, string member)
            => results.Any(r => r.MemberNames.Contains(member));

        [Fact]
        public void ValidDto_HasNoErrors() => Assert.Empty(Validate(Valid()));

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Name_MissingOrBlank_IsInvalid(string value)
        {
            var dto = Valid();
            dto.Name = value;

            Assert.True(Invalid(Validate(dto), nameof(UserDto.Name)));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Login_MissingOrBlank_IsInvalid(string value)
        {
            var dto = Valid();
            dto.Login = value;

            Assert.True(Invalid(Validate(dto), nameof(UserDto.Login)));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("not-an-email")]
        public void Email_MissingOrMalformed_IsInvalid(string value)
        {
            var dto = Valid();
            dto.Email = value;

            Assert.True(Invalid(Validate(dto), nameof(UserDto.Email)));
        }

        [Fact]
        public void Password_Null_IsValid_BecauseItIsOptionalOnEdit()
        {
            var dto = Valid();
            dto.Password = null;

            Assert.Empty(Validate(dto));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Password_BlankWhenInformed_IsInvalid(string value)
        {
            var dto = Valid();
            dto.Password = value;

            Assert.True(Invalid(Validate(dto), nameof(UserDto.Password)));
        }

        [Fact]
        public void Fields_LongerThan450Characters_AreInvalid()
        {
            var dto = Valid();
            dto.Name = new string('a', 451);
            dto.Login = new string('b', 451);
            dto.Email = new string('c', 445) + "@x.com";

            var results = Validate(dto);

            Assert.True(Invalid(results, nameof(UserDto.Name)));
            Assert.True(Invalid(results, nameof(UserDto.Login)));
            Assert.True(Invalid(results, nameof(UserDto.Email)));
        }
    }
}
