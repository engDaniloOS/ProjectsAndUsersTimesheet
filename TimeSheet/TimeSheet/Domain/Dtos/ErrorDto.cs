using System.Text.Json.Serialization;

namespace TimeSheet.Domain.Dtos
{
    public class ErrorDto
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
