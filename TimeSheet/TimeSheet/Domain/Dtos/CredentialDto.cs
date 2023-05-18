using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TimeSheet.Domain.Dtos
{
    public class CredentialDto
    {
        [Required]
        [JsonPropertyName("login")]
        public string Login { get; set; }

        [Required]
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
