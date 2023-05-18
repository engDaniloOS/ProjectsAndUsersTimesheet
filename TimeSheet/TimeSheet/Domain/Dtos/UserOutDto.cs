using System.Text.Json.Serialization;

namespace TimeSheet.Domain.Dtos
{
    public class UserOutDto
    {
        [JsonPropertyName("user_id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("login")]
        public string Login { get; set; }

        [JsonIgnore]
        public string Error { get; set; }
    }
}
