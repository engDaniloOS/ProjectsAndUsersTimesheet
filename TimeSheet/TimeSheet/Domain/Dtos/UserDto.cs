using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TimeSheet.Domain.Dtos
{
    public class UserDto
    {
        [Required]
        [MaxLength(450)]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(450)]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [Required]
        [MaxLength(450)]
        [JsonPropertyName("login")]
        public string Login { get; set; }

        /// <summary>
        /// Obrigatória na criação (validada em <c>UserService.SaveNewUser</c>); opcional na edição:
        /// omitida mantém a senha atual, mas se informada não pode ser vazia ou só espaços.
        /// </summary>
        [MinLength(1, ErrorMessage = "The Password field cannot be blank.")]
        [RegularExpression(@"(?s).*\S.*", ErrorMessage = "The Password field cannot be blank.")]
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
