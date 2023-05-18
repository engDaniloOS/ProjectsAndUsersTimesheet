using System.Text.Json.Serialization;

namespace TimeSheet.Domain.Dtos
{
    public class CredentialOutDto
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("user")]
        public UserOutDto User { get; set; }

        [JsonIgnore]
        public bool IsCredentialInvalid { get; set; }

        [JsonIgnore]
        public string Error { get; set; }
    }
}
